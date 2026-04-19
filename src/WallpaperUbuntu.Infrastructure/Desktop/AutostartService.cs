using Microsoft.Extensions.Logging;
using WallpaperUbuntu.Domain.Interfaces;

namespace WallpaperUbuntu.Infrastructure.Desktop;

/// <summary>
/// 开机启动服务实现
/// </summary>
public class AutostartService : IAutostartService
{
    private readonly ILogger<AutostartService>? _logger;
    private readonly string _autostartDir;
    private readonly string _desktopFilePath;

    private const string DesktopFileContent = @"[Desktop Entry]
Type=Application
Name=WallpaperUbuntu
Comment=Ubuntu Wallpaper Manager
Exec={0}
Icon=preferences-desktop-wallpaper
Terminal=false
Categories=Utility;
X-GNOME-Autostart-enabled=true
";

    public AutostartService(ILogger<AutostartService>? logger = null)
    {
        _logger = logger;
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _autostartDir = Path.Combine(homeDir, ".config", "autostart");
        _desktopFilePath = Path.Combine(_autostartDir, "wallpaper-ubuntu.desktop");
    }

    /// <inheritdoc/>
    public Task EnableAsync(string executablePath)
    {
        try
        {
            // 确保目录存在
            if (!Directory.Exists(_autostartDir))
            {
                Directory.CreateDirectory(_autostartDir);
            }

            // 写入 .desktop 文件
            var content = string.Format(DesktopFileContent, executablePath);
            File.WriteAllText(_desktopFilePath, content);
            
            _logger?.LogInformation("已启用开机启动: {Path}", _desktopFilePath);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "启用开机启动失败");
            throw;
        }
    }

    /// <inheritdoc/>
    public Task DisableAsync()
    {
        try
        {
            if (File.Exists(_desktopFilePath))
            {
                File.Delete(_desktopFilePath);
                _logger?.LogInformation("已禁用开机启动");
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "禁用开机启动失败");
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<bool> IsEnabledAsync()
    {
        var exists = File.Exists(_desktopFilePath);
        return Task.FromResult(exists);
    }
}
