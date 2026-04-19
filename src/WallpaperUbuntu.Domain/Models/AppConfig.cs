namespace WallpaperUbuntu.Domain.Models;

/// <summary>
/// 应用配置模型
/// </summary>
public class AppConfig
{
    /// <summary>
    /// 壁纸文件夹列表
    /// </summary>
    public List<string> Folders { get; set; } = new();

    /// <summary>
    /// 是否递归扫描子目录
    /// </summary>
    public bool RecursiveScan { get; set; } = true;

    /// <summary>
    /// 切换间隔（秒）
    /// </summary>
    public int SwitchIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// 切换模式：Sequential, Random, NonRepeatingRandom
    /// </summary>
    public string SwitchMode { get; set; } = "Sequential";

    /// <summary>
    /// 是否启用开机启动
    /// </summary>
    public bool AutoStartEnabled { get; set; }

    /// <summary>
    /// 是否启用自动切换
    /// </summary>
    public bool AutoSwitchEnabled { get; set; } = true;

    /// <summary>
    /// 是否启动时最小化
    /// </summary>
    public bool StartMinimized { get; set; }

    /// <summary>
    /// 支持的图片扩展名
    /// </summary>
    public List<string> SupportedExtensions { get; set; } = new() { ".jpg", ".jpeg", ".png", ".webp" };

    /// <summary>
    /// 创建默认配置
    /// </summary>
    public static AppConfig CreateDefault()
    {
        return new AppConfig
        {
            Folders = new List<string>(),
            RecursiveScan = true,
            SwitchIntervalSeconds = 300,
            SwitchMode = "Sequential",
            AutoStartEnabled = false,
            AutoSwitchEnabled = true,
            StartMinimized = false,
            SupportedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".webp" }
        };
    }
}
