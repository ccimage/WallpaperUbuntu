using WallpaperUbuntu.Domain.Models;

namespace WallpaperUbuntu.Domain.Interfaces;

/// <summary>
/// 壁纸扫描接口
/// </summary>
public interface IWallpaperScanner
{
    /// <summary>
    /// 扫描壁纸
    /// </summary>
    /// <param name="folders">文件夹列表</param>
    /// <param name="recursive">是否递归</param>
    /// <param name="extensions">支持的扩展名</param>
    /// <returns>壁纸列表</returns>
    Task<List<WallpaperItem>> ScanAsync(
        IEnumerable<string> folders, 
        bool recursive, 
        IEnumerable<string> extensions);
}
