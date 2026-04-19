namespace WallpaperUbuntu.Domain.Interfaces;

/// <summary>
/// 壁纸系统服务接口
/// </summary>
public interface IWallpaperSystemService
{
    /// <summary>
    /// 设置壁纸
    /// </summary>
    /// <param name="filePath">图片文件路径</param>
    Task SetWallpaperAsync(string filePath);

    /// <summary>
    /// 获取当前壁纸
    /// </summary>
    /// <returns>当前壁纸路径，如果没有则返回null</returns>
    Task<string?> GetCurrentWallpaperAsync();
}
