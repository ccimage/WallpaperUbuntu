using System.Text.Json;
using WallpaperUbuntu.Domain.Interfaces;
using WallpaperUbuntu.Domain.Models;

namespace WallpaperUbuntu.Infrastructure.Configuration;

/// <summary>
/// JSON配置存储实现
/// </summary>
public class JsonConfigStore : IConfigStore
{
    private readonly string _configPath;
    private readonly string _configDir;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonConfigStore()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _configDir = Path.Combine(homeDir, ".config", "WallpaperUbuntu");
        _configPath = Path.Combine(_configDir, "config.json");
    }

    /// <inheritdoc/>
    public async Task<AppConfig> LoadAsync()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                var defaultConfig = AppConfig.CreateDefault();
                await SaveAsync(defaultConfig);
                return defaultConfig;
            }

            var json = await File.ReadAllTextAsync(_configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
            
            return config ?? AppConfig.CreateDefault();
        }
        catch (Exception)
        {
            // 配置损坏时返回默认配置
            return AppConfig.CreateDefault();
        }
    }

    /// <inheritdoc/>
    public async Task SaveAsync(AppConfig config)
    {
        try
        {
            // 确保目录存在
            if (!Directory.Exists(_configDir))
            {
                Directory.CreateDirectory(_configDir);
            }

            // 原子写入：先写入临时文件，再替换
            var tempPath = _configPath + ".tmp";
            var json = JsonSerializer.Serialize(config, JsonOptions);
            
            await File.WriteAllTextAsync(tempPath, json);
            
            // 备份旧配置
            if (File.Exists(_configPath))
            {
                var backupPath = _configPath + ".bak";
                File.Copy(_configPath, backupPath, true);
            }
            
            // 替换配置文件
            File.Move(tempPath, _configPath, true);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
