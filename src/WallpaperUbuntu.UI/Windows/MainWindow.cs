using Gtk;
using WallpaperUbuntu.Application.Services;
using WallpaperUbuntu.Domain.Models;
using WallpaperUbuntu.UI.Dialogs;

namespace WallpaperUbuntu.UI.Windows;

/// <summary>
/// 主窗口
/// </summary>
public class MainWindow : Window
{
    private readonly WallpaperAppService _appService;
    
    // 控件
    private ListBox _folderListBox;
    private Button _addFolderButton;
    private Button _removeFolderButton;
    private CheckButton _recursiveCheckButton;
    private ComboBoxText _modeComboBox;
    private SpinButton _intervalSpinButton;
    private CheckButton _autoStartCheckButton;
    private CheckButton _autoSwitchCheckButton;
    private Button _nextButton;
    private Button _previousButton;
    private Button _pauseButton;
    private Button _resumeButton;
    private Label _statusLabel;
    private Label _wallpaperNameLabel;
    private Label _wallpaperPathLabel;
    
    /// <summary>
    /// 是否最小化到托盘
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    public MainWindow(WallpaperAppService appService) : base("WallpaperUbuntu")
    {
        _appService = appService;
        SetDefaultSize(800, 600);
        SetPosition(WindowPosition.Center);
        
        // 订阅状态变更事件
        _appService.StateChanged += OnStateChanged;
        
        // 创建主布局
        var mainBox = new Box(Orientation.Vertical, 0);
        
        // 创建菜单栏
        var menuBar = CreateMenuBar();
        mainBox.PackStart(menuBar, false, false, 0);
        
        // 内容区域（带边距）
        var contentContainer = new Box(Orientation.Vertical, 5);
        contentContainer.Margin = 10;
        
        // 创建内容区域
        var contentBox = new Box(Orientation.Horizontal, 10);
        
        // 左侧：配置区域
        var leftBox = CreateLeftPanel();
        contentBox.PackStart(leftBox, true, true, 0);
        
        // 右侧：状态区域
        var rightBox = CreateRightPanel();
        contentBox.PackStart(rightBox, true, true, 0);
        
        contentContainer.PackStart(contentBox, true, true, 0);
        
        // 控制按钮区域
        var controlBox = CreateControlPanel();
        contentContainer.PackStart(controlBox, false, false, 5);
        
        // 状态栏
        _statusLabel = new Label("就绪");
        _statusLabel.Halign = Align.Start;
        contentContainer.PackStart(_statusLabel, false, false, 5);
        
        mainBox.PackStart(contentContainer, true, true, 0);
        
        Add(mainBox);
        
        // 绑定事件
        BindEvents();
        
        // 加载初始数据
        LoadData();
        
        DeleteEvent += OnDeleteEvent;
        WindowStateEvent += OnWindowStateEvent;
    }

    /// <summary>
    /// 创建菜单栏
    /// </summary>
    private MenuBar CreateMenuBar()
    {
        var menuBar = new MenuBar();
        
        // 帮助菜单
        var helpMenu = new Menu();
        var helpMenuItem = new MenuItem("帮助");
        helpMenuItem.Submenu = helpMenu;
        
        // 关于菜单项
        var aboutMenuItem = new MenuItem("关于");
        aboutMenuItem.Activated += OnAboutClicked;
        helpMenu.Append(aboutMenuItem);
        
        menuBar.Append(helpMenuItem);
        
        return menuBar;
    }

    /// <summary>
    /// 关于菜单点击事件
    /// </summary>
    private void OnAboutClicked(object? sender, EventArgs e)
    {
        Dialogs.AppAboutDialog.ShowAbout(this);
    }

    private Box CreateLeftPanel()
    {
        var box = new Box(Orientation.Vertical, 5);
        
        // 文件夹列表
        var folderFrame = new Frame("壁纸文件夹");
        _folderListBox = new ListBox();
        _folderListBox.SelectionMode = SelectionMode.Single;
        folderFrame.Add(_folderListBox);
        box.PackStart(folderFrame, true, true, 0);
        
        // 文件夹按钮
        var folderButtonBox = new Box(Orientation.Horizontal, 5);
        _addFolderButton = new Button("添加文件夹");
        _removeFolderButton = new Button("移除文件夹");
        folderButtonBox.PackStart(_addFolderButton, true, true, 0);
        folderButtonBox.PackStart(_removeFolderButton, true, true, 0);
        box.PackStart(folderButtonBox, false, false, 0);
        
        // 递归扫描
        _recursiveCheckButton = new CheckButton("递归扫描子目录");
        box.PackStart(_recursiveCheckButton, false, false, 5);
        
        // 切换模式
        var modeFrame = new Frame("切换设置");
        var modeBox = new Box(Orientation.Vertical, 5);
        modeBox.Margin = 5;
        
        var modeLabel = new Label("切换模式:");
        modeLabel.Halign = Align.Start;
        modeBox.PackStart(modeLabel, false, false, 0);
        
        _modeComboBox = new ComboBoxText();
        _modeComboBox.AppendText("Sequential");
        _modeComboBox.AppendText("Random");
        _modeComboBox.AppendText("NonRepeatingRandom");
        modeBox.PackStart(_modeComboBox, false, false, 0);
        
        var intervalLabel = new Label("切换间隔 (秒):");
        intervalLabel.Halign = Align.Start;
        modeBox.PackStart(intervalLabel, false, false, 0);
        
        _intervalSpinButton = new SpinButton(10, 86400, 1);
        modeBox.PackStart(_intervalSpinButton, false, false, 0);
        
        modeFrame.Add(modeBox);
        box.PackStart(modeFrame, false, false, 5);
        
        // 启动设置
        var startFrame = new Frame("启动设置");
        var startBox = new Box(Orientation.Vertical, 5);
        startBox.Margin = 5;
        
        _autoStartCheckButton = new CheckButton("开机自动启动");
        startBox.PackStart(_autoStartCheckButton, false, false, 0);
        
        _autoSwitchCheckButton = new CheckButton("启动时自动切换壁纸");
        startBox.PackStart(_autoSwitchCheckButton, false, false, 0);
        
        startFrame.Add(startBox);
        box.PackStart(startFrame, false, false, 5);
        
        return box;
    }

    private Box CreateRightPanel()
    {
        var box = new Box(Orientation.Vertical, 5);
        
        // 当前壁纸信息
        var infoFrame = new Frame("当前壁纸");
        var infoBox = new Box(Orientation.Vertical, 5);
        infoBox.Margin = 10;
        
        _wallpaperNameLabel = new Label("未选择壁纸");
        _wallpaperNameLabel.Halign = Align.Start;
        infoBox.PackStart(_wallpaperNameLabel, false, false, 0);
        
        _wallpaperPathLabel = new Label("");
        _wallpaperPathLabel.Halign = Align.Start;
        _wallpaperPathLabel.Ellipsize = Pango.EllipsizeMode.Middle;
        infoBox.PackStart(_wallpaperPathLabel, false, false, 0);
        
        infoFrame.Add(infoBox);
        box.PackStart(infoFrame, false, false, 0);
        
        // 统计信息
        var statsFrame = new Frame("统计");
        var statsBox = new Box(Orientation.Vertical, 5);
        statsBox.Margin = 10;
        
        var totalLabel = new Label("壁纸总数: 0");
        totalLabel.Halign = Align.Start;
        statsBox.PackStart(totalLabel, false, false, 0);
        
        statsFrame.Add(statsBox);
        box.PackStart(statsFrame, false, false, 0);
        
        return box;
    }

    private Box CreateControlPanel()
    {
        var box = new Box(Orientation.Horizontal, 5);
        
        _previousButton = new Button("上一张");
        _nextButton = new Button("下一张");
        _pauseButton = new Button("暂停");
        _resumeButton = new Button("恢复");
        
        box.PackStart(_previousButton, true, true, 0);
        box.PackStart(_nextButton, true, true, 0);
        box.PackStart(_pauseButton, true, true, 0);
        box.PackStart(_resumeButton, true, true, 0);
        
        return box;
    }

    private void BindEvents()
    {
        _addFolderButton.Clicked += OnAddFolderClicked;
        _removeFolderButton.Clicked += OnRemoveFolderClicked;
        _recursiveCheckButton.Toggled += OnRecursiveToggled;
        _modeComboBox.Changed += OnModeChanged;
        _intervalSpinButton.ValueChanged += OnIntervalChanged;
        _autoStartCheckButton.Toggled += OnAutoStartToggled;
        _autoSwitchCheckButton.Toggled += OnAutoSwitchToggled;
        
        _nextButton.Clicked += async (s, e) => await _appService.NextWallpaperAsync();
        _previousButton.Clicked += async (s, e) => await _appService.PreviousWallpaperAsync();
        _pauseButton.Clicked += (s, e) => _appService.PauseAutoSwitch();
        _resumeButton.Clicked += (s, e) => _appService.ResumeAutoSwitch();
    }

    private void LoadData()
    {
        var config = _appService.Config;
        var state = _appService.State;
        
        // 加载文件夹列表
        RefreshFolderList();
        
        // 加载设置
        _recursiveCheckButton.Active = config.RecursiveScan;
        _intervalSpinButton.Value = config.SwitchIntervalSeconds;
        _autoStartCheckButton.Active = config.AutoStartEnabled;
        _autoSwitchCheckButton.Active = config.AutoSwitchEnabled;
        
        // 设置模式
        var modeIndex = config.SwitchMode.ToLower() switch
        {
            "random" => 1,
            "nonrepeatingrandom" => 2,
            _ => 0
        };
        _modeComboBox.Active = modeIndex;
        
        // 更新状态显示
        UpdateStatusDisplay();
    }

    private void RefreshFolderList()
    {
        // 清空列表
        foreach (var child in _folderListBox.Children)
        {
            _folderListBox.Remove(child);
        }
        
        // 添加文件夹
        foreach (var folder in _appService.Config.Folders)
        {
            var label = new Label(folder);
            label.Halign = Align.Start;
            _folderListBox.Add(label);
        }
        
        _folderListBox.ShowAll();
    }

    private void UpdateStatusDisplay()
    {
        var state = _appService.State;
        var current = state.CurrentWallpaper;
        
        if (current != null)
        {
            _wallpaperNameLabel.Text = $"文件名: {current.FileName}";
            _wallpaperPathLabel.Text = $"路径: {current.FilePath}";
        }
        else
        {
            _wallpaperNameLabel.Text = "未选择壁纸";
            _wallpaperPathLabel.Text = "";
        }
        
        _statusLabel.Text = $"壁纸总数: {state.TotalCount} | 自动切换: {(state.IsPaused ? "已暂停" : "运行中")}";
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        // 在主线程更新UI
        GLib.Idle.Add(() =>
        {
            UpdateStatusDisplay();
            return false;
        });
    }

    private async void OnAddFolderClicked(object? sender, EventArgs e)
    {
        using var dialog = new FileChooserDialog(
            "选择壁纸文件夹",
            this,
            FileChooserAction.SelectFolder,
            "取消", ResponseType.Cancel,
            "选择", ResponseType.Accept);
        
        if (dialog.Run() == (int)ResponseType.Accept)
        {
            var folder = dialog.Filename;
            await _appService.AddFolderAsync(folder);
            await _appService.SaveSettingsAsync();
            RefreshFolderList();
        }
        
        dialog.Destroy();
    }

    private async void OnRemoveFolderClicked(object? sender, EventArgs e)
    {
        var selectedRow = _folderListBox.SelectedRow;
        if (selectedRow != null)
        {
            var index = selectedRow.Index;
            if (index >= 0 && index < _appService.Config.Folders.Count)
            {
                var folder = _appService.Config.Folders[index];
                await _appService.RemoveFolderAsync(folder);
                await _appService.SaveSettingsAsync();
                RefreshFolderList();
            }
        }
    }

    private async void OnRecursiveToggled(object? sender, EventArgs e)
    {
        _appService.Config.RecursiveScan = _recursiveCheckButton.Active;
        await _appService.RefreshCatalogAsync();
        await _appService.SaveSettingsAsync();
    }

    private async void OnModeChanged(object? sender, EventArgs e)
    {
        var mode = _modeComboBox.ActiveId ?? "Sequential";
        _appService.Config.SwitchMode = mode;
        _appService.UpdateStrategy(mode);
        await _appService.SaveSettingsAsync();
    }

    private async void OnIntervalChanged(object? sender, EventArgs e)
    {
        _appService.Config.SwitchIntervalSeconds = (int)_intervalSpinButton.Value;
        await _appService.SaveSettingsAsync();
    }

    private async void OnAutoStartToggled(object? sender, EventArgs e)
    {
        _appService.Config.AutoStartEnabled = _autoStartCheckButton.Active;
        await _appService.SaveSettingsAsync();
    }

    private async void OnAutoSwitchToggled(object? sender, EventArgs e)
    {
        _appService.Config.AutoSwitchEnabled = _autoSwitchCheckButton.Active;
        await _appService.SaveSettingsAsync();
    }

    private void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        if (MinimizeToTray)
        {
            // 最小化到托盘而不是退出
            a.RetVal = true;
            Hide();
        }
        else
        {
            _appService.StateChanged -= OnStateChanged;
            Gtk.Application.Quit();
        }
    }
    
    /// <summary>
    /// 窗口状态事件处理
    /// </summary>
    private void OnWindowStateEvent(object o, WindowStateEventArgs args)
    {
        // 当窗口被最小化时，隐藏到托盘
        if ((args.Event.ChangedMask & Gdk.WindowState.Iconified) != 0 &&
            (args.Event.NewWindowState & Gdk.WindowState.Iconified) != 0)
        {
            if (MinimizeToTray)
            {
                Hide();
            }
        }
    }
}
