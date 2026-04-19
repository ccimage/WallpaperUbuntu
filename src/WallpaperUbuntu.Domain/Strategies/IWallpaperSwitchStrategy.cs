using WallpaperUbuntu.Domain.Models;

namespace WallpaperUbuntu.Domain.Strategies;

/// <summary>
/// 壁纸切换策略接口
/// </summary>
public interface IWallpaperSwitchStrategy
{
    /// <summary>
    /// 获取下一张壁纸
    /// </summary>
    /// <param name="state">应用状态</param>
    /// <param name="items">壁纸列表</param>
    /// <returns>下一张壁纸项，如果没有则返回null</returns>
    WallpaperItem? GetNext(AppState state, IReadOnlyList<WallpaperItem> items);

    /// <summary>
    /// 获取上一张壁纸
    /// </summary>
    /// <param name="state">应用状态</param>
    /// <param name="items">壁纸列表</param>
    /// <returns>上一张壁纸项，如果没有则返回null</returns>
    WallpaperItem? GetPrevious(AppState state, IReadOnlyList<WallpaperItem> items);
}
