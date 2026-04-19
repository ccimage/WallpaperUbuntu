namespace WallpaperUbuntu.Domain.Models;

/// <summary>
/// 壁纸历史记录
/// </summary>
public class WallpaperHistory
{
    /// <summary>
    /// 后退栈（用于"上一张"功能）
    /// </summary>
    public Stack<string> BackStack { get; } = new();

    /// <summary>
    /// 前进栈（用于"下一张"功能，在手动后退后使用）
    /// </summary>
    public Stack<string> ForwardStack { get; } = new();

    /// <summary>
    /// 当前轮次已播放的壁纸集合（用于不重复随机模式）
    /// </summary>
    public HashSet<string> PlayedInCurrentCycle { get; } = new();

    /// <summary>
    /// 记录播放
    /// </summary>
    public void RecordPlay(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            BackStack.Push(filePath);
            ForwardStack.Clear();
            PlayedInCurrentCycle.Add(filePath);
        }
    }

    /// <summary>
    /// 后退
    /// </summary>
    public string? GoBack(string currentPath)
    {
        if (BackStack.Count > 0)
        {
            var previous = BackStack.Pop();
            if (!string.IsNullOrEmpty(currentPath))
            {
                ForwardStack.Push(currentPath);
            }
            return previous;
        }
        return null;
    }

    /// <summary>
    /// 前进
    /// </summary>
    public string? GoForward(string currentPath)
    {
        if (ForwardStack.Count > 0)
        {
            var next = ForwardStack.Pop();
            if (!string.IsNullOrEmpty(currentPath))
            {
                BackStack.Push(currentPath);
            }
            return next;
        }
        return null;
    }

    /// <summary>
    /// 重置当前轮次
    /// </summary>
    public void ResetCycle()
    {
        PlayedInCurrentCycle.Clear();
    }

    /// <summary>
    /// 清空所有历史
    /// </summary>
    public void Clear()
    {
        BackStack.Clear();
        ForwardStack.Clear();
        PlayedInCurrentCycle.Clear();
    }
}
