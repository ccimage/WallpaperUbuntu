using Gtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WallpaperUbuntu.Application.Services;
using WallpaperUbuntu.Domain.Interfaces;
using WallpaperUbuntu.Infrastructure.Configuration;
using WallpaperUbuntu.Infrastructure.Desktop;
using WallpaperUbuntu.Infrastructure.FileSystem;
using WallpaperUbuntu.UI.Tray;
using WallpaperUbuntu.UI.Windows;

namespace WallpaperUbuntu.App;

class Program
{
    private static AppTrayIcon? _trayIcon;
    
    [STAThread]
    static void Main(string[] args)
    {
        // 创建服务容器
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        var serviceProvider = services.BuildServiceProvider();
        
        // 初始化GTK
        Gtk.Application.Init();
        
        // 创建主窗口
        var appService = serviceProvider.GetRequiredService<WallpaperAppService>();
        
        // 先初始化应用服务（加载配置）
        appService.InitializeAsync().Wait();
        
        var mainWindow = new MainWindow(appService);
        
        // 创建托盘图标
        _trayIcon = new AppTrayIcon(appService);
        _trayIcon.SetMainWindow(mainWindow);
        
        // 显示托盘图标
        _trayIcon.Show();
        
        // 显示窗口
        mainWindow.ShowAll();
        
        // 运行GTK主循环
        Gtk.Application.Run();
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        // 日志
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // 基础设施服务
        services.AddSingleton<IConfigStore, JsonConfigStore>();
        services.AddSingleton<IWallpaperScanner, FileScanner>();
        services.AddSingleton<IWallpaperSystemService, GnomeWallpaperService>();
        services.AddSingleton<IAutostartService, AutostartService>();
        
        // 应用服务
        services.AddSingleton<SchedulerService>();
        services.AddSingleton<WallpaperAppService>();
    }
}
