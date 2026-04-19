using WallpaperUbuntu.Domain.Models;

namespace WallpaperUbuntu.Domain.Strategies;

/// <summary>
/// 顺序播放策略
/// </summary>
public class SequentialStrategy : IWallpaperSwitchStrategy
{
    /// <inheritdoc/>
    public WallpaperItem? GetNext(AppState state, IReadOnlyList<WallpaperItem> items)
    {
        if (items.Count == 0)
            return null;

        var nextIndex = (state.CurrentIndex + 1) % items.Count;
        return items[nextIndex];
    }

    /// <inheritdoc/>
    public WallpaperItem? GetPrevious(AppState state, IReadOnlyList<WallpaperItem> items)
    {
        if (items.Count == 0)
            return null;

        var prevIndex = state.CurrentIndex <= 0 ? items.Count - 1 : state.CurrentIndex - 1;
        return items[prevIndex];
    }
}
