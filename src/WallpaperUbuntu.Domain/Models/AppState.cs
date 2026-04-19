namespace WallpaperUbuntu.Domain.Models;

/// <summary>
/// 应用运行时状态
/// </summary>
public class AppState
{
    /// <summary>
    /// 当前壁纸路径
    /// </summary>
    public string? CurrentWallpaperPath { get; set; }

    /// <summary>
    /// 当前索引（用于顺序模式）
    /// </summary>
    public int CurrentIndex { get; set; }

    /// <summary>
    /// 当前切换模式
    /// </summary>
    public string SwitchMode { get; set; } = "Sequential";

    /// <summary>
    /// 是否启用自动切换
    /// </summary>
    public bool AutoSwitchEnabled { get; set; } = true;

    /// <summary>
    /// 是否暂停
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// 壁纸列表缓存
    /// </summary>
    public List<WallpaperItem> WallpaperList { get; set; } = new();

    /// <summary>
    /// 历史记录
    /// </summary>
    public WallpaperHistory History { get; } = new();

    /// <summary>
    /// 最后切换时间
    /// </summary>
    public DateTime? LastSwitchTime { get; set; }

    /// <summary>
    /// 当前壁纸项
    /// </summary>
    public WallpaperItem? CurrentWallpaper => WallpaperList.FirstOrDefault(w => w.FilePath == CurrentWallpaperPath);

    /// <summary>
    /// 壁纸总数
    /// </summary>
    public int TotalCount => WallpaperList.Count;
}
