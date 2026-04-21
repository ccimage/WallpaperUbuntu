# WallpaperUbuntu
> Vibe Coding is happy coding.

一个为 Ubuntu GNOME 桌面环境设计的轻量级壁纸管理工具。

## 功能特性

- 支持从本地文件夹读取壁纸
- 自动定时切换壁纸
- 支持多种切换策略：顺序、随机、不重复随机
- 提供直观的 GtkSharp 图形界面
- 支持开机自启动
- 低资源占用，后台稳定运行

## 系统要求

- Ubuntu 20.04+ (GNOME 桌面)
- .NET 8 Runtime

## 构建项目

```bash
# 克隆项目
git clone <repository-url>
cd WallpaperUbuntu

# 还原依赖
dotnet restore

# 构建
dotnet build

# 发布
dotnet publish -c Release -r linux-x64 --self-contained false


# 单个可执行文件
dotnet publish src/WallpaperUbuntu.App/WallpaperUbuntu.App.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

## 运行

```bash
# 开发模式
dotnet run --project src/WallpaperUbuntu.App

# 发布后运行
./src/WallpaperUbuntu.App/bin/Release/net8.0/linux-x64/WallpaperUbuntu.App
```

## 配置

配置文件位于 `~/.config/WallpaperUbuntu/config.json`

```json
{
  "folders": ["/home/user/Pictures/Wallpapers"],
  "recursiveScan": true,
  "switchIntervalSeconds": 300,
  "switchMode": "Sequential",
  "autoStartEnabled": false,
  "autoSwitchEnabled": true,
  "supportedExtensions": [".jpg", ".jpeg", ".png", ".webp"]
}
```

## 项目结构

```
WallpaperUbuntu/
├── src/
│   ├── WallpaperUbuntu.App/           # 应用入口
│   ├── WallpaperUbuntu.UI/            # GtkSharp UI
│   ├── WallpaperUbuntu.Application/   # 应用服务
│   ├── WallpaperUbuntu.Domain/        # 领域模型
│   └── WallpaperUbuntu.Infrastructure/ # 基础设施
└── tests/
    └── WallpaperUbuntu.Tests/         # 测试项目
```

## 技术栈

- .NET 8
- GtkSharp 3
- System.Text.Json

## 许可证

MIT License
