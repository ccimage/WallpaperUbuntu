using WallpaperUbuntu.Domain.Models;

namespace WallpaperUbuntu.Domain.Strategies;

/// <summary>
/// 随机播放策略
/// </summary>
public class RandomStrategy : IWallpaperSwitchStrategy
{
    private readonly Random _random = new();

    /// <inheritdoc/>
    public WallpaperItem? GetNext(AppState state, IReadOnlyList<WallpaperItem> items)
    {
        if (items.Count == 0)
            return null;

        if (items.Count == 1)
            return items[0];

        var randomIndex = _random.Next(items.Count);
        return items[randomIndex];
    }

    /// <inheritdoc/>
    public WallpaperItem? GetPrevious(AppState state, IReadOnlyList<WallpaperItem> items)
    {
        // 随机模式下，上一张从历史记录获取
        var previousPath = state.History.GoBack(state.CurrentWallpaperPath);
        if (previousPath != null)
        {
            return items.FirstOrDefault(w => w.FilePath == previousPath);
        }
        return null;
    }
}
