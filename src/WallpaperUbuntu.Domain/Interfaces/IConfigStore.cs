using WallpaperUbuntu.Domain.Models;

namespace WallpaperUbuntu.Domain.Interfaces;

/// <summary>
/// 配置存储接口
/// </summary>
public interface IConfigStore
{
    /// <summary>
    /// 加载配置
    /// </summary>
    Task<AppConfig> LoadAsync();

    /// <summary>
    /// 保存配置
    /// </summary>
    Task SaveAsync(AppConfig config);
}
