using Microsoft.Extensions.Logging;

namespace WallpaperUbuntu.Application.Services;

/// <summary>
/// 调度服务
/// </summary>
public class SchedulerService : IDisposable
{
    private readonly ILogger<SchedulerService>? _logger;
    private PeriodicTimer? _timer;
    private Task? _timerTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly object _lock = new();
    
    private bool _isRunning;
    private int _intervalSeconds;
    private Func<Task>? _callback;
    
    private bool _isExecuting;

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// 当前间隔（秒）
    /// </summary>
    public int IntervalSeconds => _intervalSeconds;

    public SchedulerService(ILogger<SchedulerService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// 启动调度
    /// </summary>
    /// <param name="intervalSeconds">间隔秒数</param>
    /// <param name="callback">回调函数</param>
    public void Start(int intervalSeconds, Func<Task> callback)
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                _logger?.LogWarning("调度器已在运行");
                return;
            }

            _intervalSeconds = intervalSeconds;
            _callback = callback;
            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
            _timerTask = RunAsync(_cancellationTokenSource.Token);
            _isRunning = true;
            
            _logger?.LogInformation("调度器已启动，间隔: {Interval}秒", intervalSeconds);
        }
    }

    /// <summary>
    /// 停止调度
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning)
                return;

            _cancellationTokenSource?.Cancel();
            _timer?.Dispose();
            _timer = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _isRunning = false;
            
            _logger?.LogInformation("调度器已停止");
        }
    }

    /// <summary>
    /// 更新间隔
    /// </summary>
    /// <param name="intervalSeconds">新的间隔秒数</param>
    public void UpdateInterval(int intervalSeconds)
    {
        lock (_lock)
        {
            if (!_isRunning || _callback == null)
                return;

            _intervalSeconds = intervalSeconds;
            _timer?.Dispose();
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
            
            _logger?.LogInformation("调度器间隔已更新: {Interval}秒", intervalSeconds);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_timer != null && await _timer.WaitForNextTickAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // 防止重入
                if (_isExecuting)
                {
                    _logger?.LogDebug("上次任务尚未完成，跳过本次执行");
                    continue;
                }

                _isExecuting = true;
                try
                {
                    if (_callback != null)
                    {
                        await _callback();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "调度任务执行失败");
                }
                finally
                {
                    _isExecuting = false;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消
        }
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
