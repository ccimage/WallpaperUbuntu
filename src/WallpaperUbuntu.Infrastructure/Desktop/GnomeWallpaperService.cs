using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WallpaperUbuntu.Domain.Interfaces;

namespace WallpaperUbuntu.Infrastructure.Desktop;

/// <summary>
/// GNOME壁纸服务实现
/// </summary>
public class GnomeWallpaperService : IWallpaperSystemService
{
    private readonly ILogger<GnomeWallpaperService>? _logger;

    public GnomeWallpaperService(ILogger<GnomeWallpaperService>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SetWallpaperAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("壁纸文件不存在", filePath);
        }

        // 转换为URI格式
        var uri = $"file://{Uri.EscapeDataString(filePath)}";
        
        _logger?.LogInformation("设置壁纸: {Uri}", uri);

        // 设置浅色模式壁纸
        await ExecuteGsettingsAsync("set", "org.gnome.desktop.background", "picture-uri", uri);
        
        // 设置深色模式壁纸
        await ExecuteGsettingsAsync("set", "org.gnome.desktop.background", "picture-uri-dark", uri);
    }

    /// <inheritdoc/>
    public async Task<string?> GetCurrentWallpaperAsync()
    {
        try
        {
            var result = await ExecuteGsettingsAsync("get", "org.gnome.desktop.background", "picture-uri");
            
            // 解析返回值，格式为: 'file:///path/to/image.jpg'
            if (result.StartsWith("'file://"))
            {
                // 移除引号和 file:// 前缀
                var uri = result.Trim('\'').Substring(7);
                return Uri.UnescapeDataString(uri);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "获取当前壁纸失败");
            return null;
        }
    }

    private async Task<string> ExecuteGsettingsAsync(string command, string schema, string key, string? value = null)
    {
        var args = new List<string> { command, schema, key };
        if (value != null)
        {
            args.Add(value);
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "gsettings",
                Arguments = string.Join(" ", args),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            _logger?.LogError("gsettings 执行失败: {Error}", error);
            throw new InvalidOperationException($"gsettings 执行失败: {error}");
        }

        return output.Trim();
    }
}
