using WallpaperUbuntu.Domain.Models;

namespace WallpaperUbuntu.Domain.Strategies;

/// <summary>
/// 不重复随机播放策略
/// </summary>
public class NonRepeatingRandomStrategy : IWallpaperSwitchStrategy
{
    private readonly Random _random = new();

    /// <inheritdoc/>
    public WallpaperItem? GetNext(AppState state, IReadOnlyList<WallpaperItem> items)
    {
        if (items.Count == 0)
            return null;

        if (items.Count == 1)
            return items[0];

        // 如果所有壁纸都已播放，重置轮次
        var availableItems = items.Where(w => !state.History.PlayedInCurrentCycle.Contains(w.FilePath)).ToList();
        
        if (availableItems.Count == 0)
        {
            state.History.ResetCycle();
            availableItems = items.ToList();
        }

        // 排除当前壁纸
        if (!string.IsNullOrEmpty(state.CurrentWallpaperPath))
        {
            availableItems = availableItems.Where(w => w.FilePath != state.CurrentWallpaperPath).ToList();
        }

        if (availableItems.Count == 0)
            return items.FirstOrDefault(w => w.FilePath != state.CurrentWallpaperPath);

        var randomIndex = _random.Next(availableItems.Count);
        return availableItems[randomIndex];
    }

    /// <inheritdoc/>
    public WallpaperItem? GetPrevious(AppState state, IReadOnlyList<WallpaperItem> items)
    {
        var previousPath = state.History.GoBack(state.CurrentWallpaperPath);
        if (previousPath != null)
        {
            return items.FirstOrDefault(w => w.FilePath == previousPath);
        }
        return null;
    }
}
