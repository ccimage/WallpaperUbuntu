namespace WallpaperUbuntu.Domain.Models;

/// <summary>
/// 壁纸项模型
/// </summary>
public class WallpaperItem
{
    /// <summary>
    /// 文件完整路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// 图片宽度（可选）
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// 图片高度（可选）
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// 所属文件夹
    /// </summary>
    public string Folder => Path.GetDirectoryName(FilePath) ?? string.Empty;

    /// <summary>
    /// 分辨率描述
    /// </summary>
    public string Resolution => Width.HasValue && Height.HasValue 
        ? $"{Width}x{Height}" 
        : "未知";
}
