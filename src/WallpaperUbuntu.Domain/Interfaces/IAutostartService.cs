namespace WallpaperUbuntu.Domain.Interfaces;

/// <summary>
/// 开机启动服务接口
/// </summary>
public interface IAutostartService
{
    /// <summary>
    /// 启用开机启动
    /// </summary>
    /// <param name="executablePath">可执行文件路径</param>
    Task EnableAsync(string executablePath);

    /// <summary>
    /// 禁用开机启动
    /// </summary>
    Task DisableAsync();

    /// <summary>
    /// 检查是否已启用
    /// </summary>
    Task<bool> IsEnabledAsync();
}
