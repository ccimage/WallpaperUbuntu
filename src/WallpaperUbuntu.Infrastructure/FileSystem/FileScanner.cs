using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WallpaperUbuntu.Domain.Interfaces;
using WallpaperUbuntu.Domain.Models;

namespace WallpaperUbuntu.Infrastructure.FileSystem;

/// <summary>
/// 文件扫描实现
/// </summary>
public class FileScanner : IWallpaperScanner
{
    private readonly ILogger<FileScanner>? _logger;

    public FileScanner(ILogger<FileScanner>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<WallpaperItem>> ScanAsync(
        IEnumerable<string> folders, 
        bool recursive, 
        IEnumerable<string> extensions)
    {
        var result = new List<WallpaperItem>();
        var extensionSet = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);

        foreach (var folder in folders)
        {
            try
            {
                if (!Directory.Exists(folder))
                {
                    _logger?.LogWarning("目录不存在: {Folder}", folder);
                    continue;
                }

                var items = await Task.Run(() => ScanFolder(folder, recursive, extensionSet));
                result.AddRange(items);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger?.LogWarning(ex, "无权限访问目录: {Folder}", folder);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "扫描目录失败: {Folder}", folder);
            }
        }

        _logger?.LogInformation("扫描完成，共找到 {Count} 张壁纸", result.Count);
        return result;
    }

    private List<WallpaperItem> ScanFolder(string folder, bool recursive, HashSet<string> extensionSet)
    {
        var result = new List<WallpaperItem>();
        
        try
        {
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(folder, "*.*", searchOption);

            foreach (var file in files)
            {
                try
                {
                    var extension = Path.GetExtension(file);
                    if (!extensionSet.Contains(extension))
                        continue;

                    var fileInfo = new FileInfo(file);
                    
                    result.Add(new WallpaperItem
                    {
                        FilePath = file,
                        FileName = fileInfo.Name,
                        FileSize = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime
                    });
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "跳过文件: {File}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "扫描文件夹失败: {Folder}", folder);
        }

        return result;
    }
}
