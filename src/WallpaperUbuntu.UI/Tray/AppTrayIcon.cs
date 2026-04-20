using Gtk;
using WallpaperUbuntu.Application.Services;
using WallpaperUbuntu.UI.Windows;

namespace WallpaperUbuntu.UI.Tray;

/// <summary>
/// 系统托盘图标管理
/// </summary>
public class AppTrayIcon : IDisposable
{
    private readonly WallpaperAppService _appService;
    private readonly StatusIcon _statusIcon;
    private readonly Menu _popupMenu;
    private MainWindow? _mainWindow;
    
    // 菜单项
    private MenuItem _nextMenuItem = null!;
    private MenuItem _previousMenuItem = null!;
    private MenuItem _pauseResumeMenuItem = null!;
    private MenuItem _showWindowMenuItem = null!;
    private MenuItem _exitMenuItem = null!;

    /// <summary>
    /// 托盘图标是否可用
    /// </summary>
    public bool IsAvailable => _statusIcon != null;

    public AppTrayIcon(WallpaperAppService appService)
    {
        _appService = appService;
        
        // 创建状态图标
        _statusIcon = new StatusIcon();
        _statusIcon.TooltipText = "WallpaperUbuntu - 壁纸切换工具";
        
        // 设置图标（使用系统图标或默认图标）
        try
        {
            // 尝试使用系统图标
            _statusIcon.IconName = "preferences-desktop-wallpaper";
        }
        catch
        {
            // 如果系统图标不可用，使用默认图标
            _statusIcon.IconName = "image-x-generic";
        }
        
        // 创建右键菜单
        _popupMenu = CreatePopupMenu();
        
        // 绑定事件
        _statusIcon.PopupMenu += OnPopupMenu;
        _statusIcon.Activate += OnActivate;
        
        // 订阅应用状态变更
        _appService.StateChanged += OnStateChanged;
    }

    /// <summary>
    /// 设置主窗口引用
    /// </summary>
    public void SetMainWindow(MainWindow window)
    {
        _mainWindow = window;
    }

    /// <summary>
    /// 创建右键菜单
    /// </summary>
    private Menu CreatePopupMenu()
    {
        var menu = new Menu();
        
        // 下一张
        _nextMenuItem = new MenuItem("下一张");
        _nextMenuItem.Activated += OnNextClicked;
        menu.Append(_nextMenuItem);
        
        // 上一张
        _previousMenuItem = new MenuItem("上一张");
        _previousMenuItem.Activated += OnPreviousClicked;
        menu.Append(_previousMenuItem);
        
        // 分隔线
        menu.Append(new SeparatorMenuItem());
        
        // 暂停/恢复
        _pauseResumeMenuItem = new MenuItem("暂停");
        _pauseResumeMenuItem.Activated += OnPauseResumeClicked;
        menu.Append(_pauseResumeMenuItem);
        
        // 分隔线
        menu.Append(new SeparatorMenuItem());
        
        // 显示主窗口
        _showWindowMenuItem = new MenuItem("显示主窗口");
        _showWindowMenuItem.Activated += OnShowWindowClicked;
        menu.Append(_showWindowMenuItem);
        
        // 分隔线
        menu.Append(new SeparatorMenuItem());
        
        // 退出
        _exitMenuItem = new MenuItem("退出");
        _exitMenuItem.Activated += OnExitClicked;
        menu.Append(_exitMenuItem);
        
        menu.ShowAll();
        
        return menu;
    }

    /// <summary>
    /// 更新托盘提示文本
    /// </summary>
    public void UpdateTooltip()
    {
        var state = _appService.State;
        var current = state.CurrentWallpaper;
        
        var tooltip = "WallpaperUbuntu";
        
        if (current != null)
        {
            tooltip = $"当前壁纸: {current.FileName}";
            
            if (state.LastSwitchTime.HasValue)
            {
                var timeStr = state.LastSwitchTime.Value.ToString("HH:mm:ss");
                tooltip += $"\n切换时间: {timeStr}";
            }
            
            tooltip += $"\n状态: {(state.IsPaused ? "已暂停" : "运行中")}";
        }
        
        _statusIcon.TooltipText = tooltip;
        
        // 更新暂停/恢复菜单项文本
        _pauseResumeMenuItem.Label = state.IsPaused ? "恢复" : "暂停";
    }

    #region 事件处理

    private void OnPopupMenu(object o, PopupMenuArgs args)
    {
        UpdateTooltip();
        _popupMenu.Popup(null, null, null, 3, 0);
    }

    private void OnActivate(object o, EventArgs args)
    {
        // 左键点击显示主窗口
        ShowMainWindow();
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        await _appService.NextWallpaperAsync();
    }

    private async void OnPreviousClicked(object? sender, EventArgs e)
    {
        await _appService.PreviousWallpaperAsync();
    }

    private void OnPauseResumeClicked(object? sender, EventArgs e)
    {
        if (_appService.State.IsPaused)
        {
            _appService.ResumeAutoSwitch();
        }
        else
        {
            _appService.PauseAutoSwitch();
        }
    }

    private void OnShowWindowClicked(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        // 退出应用
        Gtk.Application.Quit();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        // 在主线程更新UI
        GLib.Idle.Add(() =>
        {
            UpdateTooltip();
            return false;
        });
    }

    #endregion

    /// <summary>
    /// 显示主窗口
    /// </summary>
    private void ShowMainWindow()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Present();
            _mainWindow.Deiconify();
        }
    }

    /// <summary>
    /// 显示托盘图标
    /// </summary>
    public void Show()
    {
        _statusIcon.Visible = true;
        UpdateTooltip();
    }

    /// <summary>
    /// 隐藏托盘图标
    /// </summary>
    public void Hide()
    {
        _statusIcon.Visible = false;
    }

    public void Dispose()
    {
        _appService.StateChanged -= OnStateChanged;
        _statusIcon?.Dispose();
        _popupMenu?.Dispose();
        GC.SuppressFinalize(this);
    }
}
