using Gtk;

namespace WallpaperUbuntu.UI.Dialogs;

/// <summary>
/// 消息严重程度
/// </summary>
public enum MessageSeverity
{
    /// <summary>
    /// 错误消息
    /// </summary>
    Error,
    
    /// <summary>
    /// 警告消息
    /// </summary>
    Warning,
    
    /// <summary>
    /// 信息消息
    /// </summary>
    Info
}

/// <summary>
/// 错误提示对话框
/// 用于向用户显示错误、警告或信息消息
/// </summary>
public class ErrorDialog : MessageDialog
{
    /// <summary>
    /// 获取消息严重程度
    /// </summary>
    public MessageSeverity Severity { get; }

    /// <summary>
    /// 创建错误对话框
    /// </summary>
    /// <param name="parent">父窗口，可为null</param>
    /// <param name="title">错误标题</param>
    /// <param name="message">错误消息</param>
    /// <param name="severity">消息严重程度，默认为错误</param>
    public ErrorDialog(Window? parent, string title, string message, MessageSeverity severity = MessageSeverity.Error)
        : base(
            parent,
            DialogFlags.Modal | DialogFlags.DestroyWithParent,
            GetMessageType(severity),
            ButtonsType.Ok,
            $"%s\n\n%s", title, message)
    {
        Severity = severity;
        Title = GetDialogTitle(severity);
        SetPosition(WindowPosition.CenterOnParent);
        
        // 设置默认响应
        AddButton("确定", ResponseType.Ok);
        DefaultResponse = ResponseType.Ok;
    }

    /// <summary>
    /// 显示错误对话框并等待用户关闭
    /// </summary>
    /// <returns>用户是否点击了确定按钮</returns>
    public new bool Run()
    {
        var response = base.Run();
        return response == (int)ResponseType.Ok;
    }

    /// <summary>
    /// 显示错误对话框的静态便捷方法
    /// </summary>
    /// <param name="parent">父窗口，可为null</param>
    /// <param name="title">错误标题</param>
    /// <param name="message">错误消息</param>
    public static void Show(Window? parent, string title, string message)
    {
        using var dialog = new ErrorDialog(parent, title, message, MessageSeverity.Error);
        dialog.Run();
    }

    /// <summary>
    /// 显示消息对话框的静态便捷方法（支持不同严重程度）
    /// </summary>
    /// <param name="parent">父窗口，可为null</param>
    /// <param name="title">消息标题</param>
    /// <param name="message">消息内容</param>
    /// <param name="severity">消息严重程度</param>
    public static void Show(Window? parent, string title, string message, MessageSeverity severity)
    {
        using var dialog = new ErrorDialog(parent, title, message, severity);
        dialog.Run();
    }

    /// <summary>
    /// 显示错误对话框的静态便捷方法（带格式化消息）
    /// </summary>
    /// <param name="parent">父窗口，可为null</param>
    /// <param name="title">错误标题</param>
    /// <param name="format">消息格式字符串</param>
    /// <param name="args">格式化参数</param>
    public static void Show(Window? parent, string title, string format, params object[] args)
    {
        var message = string.Format(format, args);
        Show(parent, title, message, MessageSeverity.Error);
    }

    /// <summary>
    /// 显示错误消息
    /// </summary>
    /// <param name="parent">父窗口，可为null</param>
    /// <param name="title">错误标题</param>
    /// <param name="message">错误消息</param>
    public static void ShowError(Window? parent, string title, string message)
    {
        Show(parent, title, message, MessageSeverity.Error);
    }

    /// <summary>
    /// 显示警告消息
    /// </summary>
    /// <param name="parent">父窗口，可为null</param>
    /// <param name="title">警告标题</param>
    /// <param name="message">警告消息</param>
    public static void ShowWarning(Window? parent, string title, string message)
    {
        Show(parent, title, message, MessageSeverity.Warning);
    }

    /// <summary>
    /// 显示信息消息
    /// </summary>
    /// <param name="parent">父窗口，可为null</param>
    /// <param name="title">信息标题</param>
    /// <param name="message">信息消息</param>
    public static void ShowInfo(Window? parent, string title, string message)
    {
        Show(parent, title, message, MessageSeverity.Info);
    }

    /// <summary>
    /// 根据严重程度获取GTK消息类型
    /// </summary>
    private static MessageType GetMessageType(MessageSeverity severity)
    {
        return severity switch
        {
            MessageSeverity.Warning => MessageType.Warning,
            MessageSeverity.Info => MessageType.Info,
            _ => MessageType.Error
        };
    }

    /// <summary>
    /// 根据严重程度获取对话框标题
    /// </summary>
    private static string GetDialogTitle(MessageSeverity severity)
    {
        return severity switch
        {
            MessageSeverity.Warning => "警告",
            MessageSeverity.Info => "信息",
            _ => "错误"
        };
    }
}
