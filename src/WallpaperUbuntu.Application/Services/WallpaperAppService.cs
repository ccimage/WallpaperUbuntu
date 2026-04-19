using Microsoft.Extensions.Logging;
using WallpaperUbuntu.Domain.Interfaces;
using WallpaperUbuntu.Domain.Models;
using WallpaperUbuntu.Domain.Strategies;

namespace WallpaperUbuntu.Application.Services;

/// <summary>
/// 壁纸应用核心服务
/// </summary>
public class WallpaperAppService : IDisposable
{
    private readonly ILogger<WallpaperAppService>? _logger;
    private readonly IConfigStore _configStore;
    private readonly IWallpaperScanner _scanner;
    private readonly IWallpaperSystemService _wallpaperService;
    private readonly IAutostartService _autostartService;
    private readonly SchedulerService _scheduler;
    
    private AppConfig _config;
    private readonly AppState _state;
    private IWallpaperSwitchStrategy _strategy;

    /// <summary>
    /// 应用状态
    /// </summary>
    public AppState State => _state;

    /// <summary>
    /// 当前配置
    /// </summary>
    public AppConfig Config => _config;

    /// <summary>
    /// 状态变更事件
    /// </summary>
    public event EventHandler? StateChanged;

    public WallpaperAppService(
        IConfigStore configStore,
        IWallpaperScanner scanner,
        IWallpaperSystemService wallpaperService,
        IAutostartService autostartService,
        SchedulerService scheduler,
        ILogger<WallpaperAppService>? logger = null)
    {
        _configStore = configStore;
        _scanner = scanner;
        _wallpaperService = wallpaperService;
        _autostartService = autostartService;
        _scheduler = scheduler;
        _logger = logger;
        
        _config = AppConfig.CreateDefault();
        _state = new AppState();
        _strategy = new SequentialStrategy();
    }

    /// <summary>
    /// 初始化应用
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger?.LogInformation("正在初始化应用...");

        // 加载配置
        _config = await _configStore.LoadAsync();
        
        // 设置策略
        UpdateStrategy(_config.SwitchMode);
        
        // 扫描壁纸
        await RefreshCatalogAsync();
        
        // 如果启用自动切换，启动调度器
        if (_config.AutoSwitchEnabled && _state.WallpaperList.Count > 0)
        {
            StartAutoSwitch();
        }
        
        _logger?.LogInformation("应用初始化完成，共 {Count} 张壁纸", _state.WallpaperList.Count);
    }

    /// <summary>
    /// 刷新壁纸目录
    /// </summary>
    public async Task RefreshCatalogAsync()
    {
        _logger?.LogInformation("正在扫描壁纸目录...");
        
        var items = await _scanner.ScanAsync(
            _config.Folders,
            _config.RecursiveScan,
            _config.SupportedExtensions);
        
        _state.WallpaperList = items;
        _state.History.Clear();
        
        // 如果有壁纸但没有当前壁纸，设置第一张
        if (items.Count > 0 && string.IsNullOrEmpty(_state.CurrentWallpaperPath))
        {
            _state.CurrentIndex = 0;
            _state.CurrentWallpaperPath = items[0].FilePath;
        }
        
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 下一张壁纸
    /// </summary>
    public async Task NextWallpaperAsync()
    {
        if (_state.WallpaperList.Count == 0)
        {
            _logger?.LogWarning("没有可用的壁纸");
            return;
        }

        var next = _strategy.GetNext(_state, _state.WallpaperList);
        if (next != null)
        {
            await SetWallpaperAsync(next);
        }
    }

    /// <summary>
    /// 上一张壁纸
    /// </summary>
    public async Task PreviousWallpaperAsync()
    {
        if (_state.WallpaperList.Count == 0)
        {
            _logger?.LogWarning("没有可用的壁纸");
            return;
        }

        var previous = _strategy.GetPrevious(_state, _state.WallpaperList);
        if (previous != null)
        {
            await SetWallpaperAsync(previous);
        }
    }

    /// <summary>
    /// 设置指定壁纸
    /// </summary>
    private async Task SetWallpaperAsync(WallpaperItem item)
    {
        try
        {
            await _wallpaperService.SetWallpaperAsync(item.FilePath);
            
            // 记录历史
            _state.History.RecordPlay(_state.CurrentWallpaperPath);
            
            // 更新状态
            _state.CurrentWallpaperPath = item.FilePath;
            _state.CurrentIndex = _state.WallpaperList.IndexOf(item);
            _state.LastSwitchTime = DateTime.Now;
            
            _logger?.LogInformation("已切换壁纸: {Name}", item.FileName);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "设置壁纸失败: {Path}", item.FilePath);
        }
    }

    /// <summary>
    /// 启动自动切换
    /// </summary>
    public void StartAutoSwitch()
    {
        if (_scheduler.IsRunning)
            return;

        _state.IsPaused = false;
        _scheduler.Start(_config.SwitchIntervalSeconds, NextWallpaperAsync);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 暂停自动切换
    /// </summary>
    public void PauseAutoSwitch()
    {
        _state.IsPaused = true;
        _scheduler.Stop();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 恢复自动切换
    /// </summary>
    public void ResumeAutoSwitch()
    {
        if (!_scheduler.IsRunning)
        {
            StartAutoSwitch();
        }
        _state.IsPaused = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        await _configStore.SaveAsync(_config);
        
        // 更新调度器间隔
        if (_scheduler.IsRunning)
        {
            _scheduler.UpdateInterval(_config.SwitchIntervalSeconds);
        }
        
        // 更新开机启动
        if (_config.AutoStartEnabled)
        {
            var execPath = Environment.ProcessPath;
            if (execPath != null)
            {
                await _autostartService.EnableAsync(execPath);
            }
        }
        else
        {
            await _autostartService.DisableAsync();
        }
        
        _logger?.LogInformation("设置已保存");
    }

    /// <summary>
    /// 更新切换策略
    /// </summary>
    public void UpdateStrategy(string mode)
    {
        _strategy = mode.ToLower() switch
        {
            "random" => new RandomStrategy(),
            "nonrepeatingrandom" => new NonRepeatingRandomStrategy(),
            _ => new SequentialStrategy()
        };
        
        _state.SwitchMode = mode;
        _logger?.LogInformation("切换策略已更新: {Mode}", mode);
    }

    /// <summary>
    /// 添加文件夹
    /// </summary>
    public async Task AddFolderAsync(string folder)
    {
        if (!_config.Folders.Contains(folder))
        {
            _config.Folders.Add(folder);
            await RefreshCatalogAsync();
        }
    }

    /// <summary>
    /// 移除文件夹
    /// </summary>
    public async Task RemoveFolderAsync(string folder)
    {
        if (_config.Folders.Remove(folder))
        {
            await RefreshCatalogAsync();
        }
    }

    public void Dispose()
    {
        _scheduler.Dispose();
        GC.SuppressFinalize(this);
    }
}
