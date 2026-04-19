# WallpaperUbuntu 设计文档

## 1. 项目概述

**项目名称**：WallpaperUbuntu  
**技术栈**：.NET 8 + GtkSharp  
**目标平台**：Ubuntu 24.04+ (GNOME 桌面)  
**核心目标**：提供一个轻量级、稳定的本地壁纸自动切换工具

## 2. 系统架构

系统采用分层架构设计：

```
+------------------------------------------------------+
|                    GtkSharp UI                       |
|  MainWindow / SettingsView / Tray Menu / Dialogs     |
+--------------------------+---------------------------+
                           |
+------------------------------------------------------+
|                 Application Services                 |
| WallpaperAppService / SchedulerService / AppState    |
+--------------------------+---------------------------+
                           |
+------------------------------------------------------+
|                    Domain Layer                      |
| WallpaperCatalog / PlaylistStrategy / ConfigModel    |
| WallpaperHistory / Validation Rules                  |
+--------------------------+---------------------------+
                           |
+------------------------------------------------------+
|                Infrastructure Layer                  |
| GnomeWallpaperService / FileScanner                  |
| JsonConfigStore / AutostartService / Logger          |
+------------------------------------------------------+
```

## 3. 核心模块设计

### 3.1 UI 模块

#### MainWindow
- 展示当前壁纸信息
- 文件夹列表管理（添加/删除）
- 切换间隔配置
- 切换策略选择
- 手动控制按钮（上一张/下一张/暂停/恢复）
- 开机启动设置

#### 托盘菜单（可选）
- 下一张/上一张
- 暂停/恢复自动切换
- 打开主窗口
- 退出程序

### 3.2 应用服务模块

#### WallpaperAppService
核心协调服务，负责：
- 应用初始化
- 响应UI操作
- 协调壁纸切换
- 状态管理

#### SchedulerService
定时切换调度：
- 启动/暂停/恢复
- 动态更新间隔
- 防止重入执行

#### AppState
运行时状态维护：
- 当前壁纸路径
- 当前索引
- 切换模式
- 自动切换状态
- 图片列表缓存

### 3.3 领域模块

#### WallpaperCatalog
壁纸文件集合管理：
- 目录扫描
- 文件过滤
- 列表生成

#### PlaylistStrategy (策略模式)
```csharp
public interface IWallpaperSwitchStrategy
{
    WallpaperItem? GetNext(AppState state, IReadOnlyList<WallpaperItem> items);
    WallpaperItem? GetPrevious(AppState state, IReadOnlyList<WallpaperItem> items);
}
```

实现类：
- `SequentialStrategy` - 顺序播放
- `RandomStrategy` - 随机播放
- `NonRepeatingRandomStrategy` - 不重复随机

#### WallpaperHistory
历史记录管理：
- 支持上一张功能
- 避免随机重复

### 3.4 基础设施模块

#### GnomeWallpaperService
通过 gsettings 设置壁纸：
```bash
gsettings set org.gnome.desktop.background picture-uri "file:///path/to/image.jpg"
```

#### FileScanner
文件扫描服务：
- 支持递归扫描
- 扩展名过滤
- 异常处理

#### JsonConfigStore
配置存储：
- 路径：`~/.config/WallpaperUbuntu/config.json`
- 原子写入
- 损坏恢复

#### AutostartService
开机启动管理：
- 生成 `.desktop` 文件到 `~/.config/autostart/`

## 4. 数据模型

### AppConfig
```csharp
public class AppConfig
{
    public List<string> Folders { get; set; } = new();
    public bool RecursiveScan { get; set; }
    public int SwitchIntervalSeconds { get; set; } = 300;
    public string SwitchMode { get; set; } = "Sequential";
    public bool AutoStartEnabled { get; set; }
    public bool AutoSwitchEnabled { get; set; }
    public bool StartMinimized { get; set; }
    public List<string> SupportedExtensions { get; set; } = new() { ".jpg", ".jpeg", ".png", ".webp" };
}
```

### WallpaperItem
```csharp
public class WallpaperItem
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}
```

## 5. 核心流程

### 启动流程
```
应用启动 -> 加载配置 -> 初始化日志 -> 扫描壁纸目录 
-> 生成壁纸列表 -> 初始化策略 -> 加载主窗口 -> 启动调度器(如启用)
```

### 切换流程
```
触发切换 -> 策略计算下一张 -> 设置系统壁纸 -> 更新状态 -> 刷新UI
```

## 6. 项目结构

```
WallpaperUbuntu/
├── src/
│   ├── WallpaperUbuntu.App/           # 入口程序
│   ├── WallpaperUbuntu.UI/            # GtkSharp UI
│   ├── WallpaperUbuntu.Application/   # 应用服务
│   ├── WallpaperUbuntu.Domain/        # 领域模型
│   ├── WallpaperUbuntu.Infrastructure/ # 基础设施
│   └── WallpaperUbuntu.Shared/        # 共享工具
├── tests/
│   └── WallpaperUbuntu.Tests/         # 测试项目
└── README.md
```

## 7. 技术要点

### 线程模型
- UI操作必须在GTK主线程
- 文件扫描在后台线程
- 使用 `GLib.Idle.Add` 更新UI

### 壁纸设置
- 使用 gsettings 命令
- 路径转URI格式
- 处理特殊字符转义

### 错误处理
- 文件夹不存在：跳过并记录
- 权限不足：跳过并告警
- 配置损坏：备份并恢复默认
- 设置失败：保持当前状态

## 8. 开发优先级

### P0 (核心)
- 配置加载与保存
- 文件夹选择与扫描
- 壁纸切换
- 自动切换调度
- GtkSharp 主窗口

### P1 (重要)
- 上一张/下一张
- 开机自启动
- 错误处理与日志
- 当前壁纸信息显示

### P2 (增强)
- 托盘支持
- 壁纸预览图
- 启动最小化
