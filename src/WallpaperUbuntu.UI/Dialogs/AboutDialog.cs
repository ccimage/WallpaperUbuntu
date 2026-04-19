using Gtk;

namespace WallpaperUbuntu.UI.Dialogs;

/// <summary>
/// 关于对话框
/// 显示应用程序信息
/// </summary>
public class AppAboutDialog : Gtk.AboutDialog
{
    /// <summary>
    /// 应用程序名称
    /// </summary>
    public const string AppName = "WallpaperUbuntu";

    /// <summary>
    /// 应用程序版本
    /// </summary>
    public const string AppVersion = "1.0.0";

    /// <summary>
    /// 应用程序描述
    /// </summary>
    public const string AppDescription = "一个轻量级的 Ubuntu 壁纸自动切换工具\n支持顺序、随机和不重复随机切换模式";

    /// <summary>
    /// 应用程序许可证
    /// </summary>
    public const string AppLicense = @"MIT License

Copyright (c) 2024 WallpaperUbuntu

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.";

    /// <summary>
    /// 应用程序网站
    /// </summary>
    public const string AppWebsite = "https://github.com/wallpaperubuntu/wallpaperubuntu";

    /// <summary>
    /// 创建关于对话框
    /// </summary>
    public AppAboutDialog()
    {
        // 设置应用程序信息
        ProgramName = AppName;
        Version = AppVersion;
        Comments = AppDescription;
        License = AppLicense;
        Website = AppWebsite;
        WebsiteLabel = "GitHub 仓库";

        // 设置作者信息
        Authors = new[] { "WallpaperUbuntu Team" };
        Artists = new[] { "WallpaperUbuntu Team" };

        // 设置版权信息
        Copyright = "Copyright © 2024 WallpaperUbuntu Team";

        // 设置窗口属性
        SetPosition(WindowPosition.CenterOnParent);
        Modal = true;
        DestroyWithParent = true;
    }

    /// <summary>
    /// 显示关于对话框的静态便捷方法
    /// </summary>
    /// <param name="parent">父窗口，可为null</param>
    public static void ShowAbout(Window? parent)
    {
        using var dialog = new AppAboutDialog();
        if (parent != null)
        {
            dialog.TransientFor = parent;
        }
        dialog.Run();
    }
}
