using System.Collections.ObjectModel;
using Godot_Manager.Helpers;
using Godot_Manager.Models;

namespace Godot_Manager.ViewModels;

/// <summary>
/// 主窗口 ViewModel，管理导航栏状态和右侧内容区域页面切换。
/// </summary>
public class MainViewModel : ViewModelBase
{
    private object? _currentView;
    private NavigationItem? _selectedNavItem;
    private bool _isNavigationExpanded = true;

    public MainViewModel()
    {
        // 初始化五个导航项（前四项顺序固定不可改动）
        NavigationItems =
        [
            new NavigationItem { Title = "项目管理", Icon = "📁", ViewKey = "ProjectList" },
            new NavigationItem { Title = "插件管理", Icon = "🔌", ViewKey = "Plugin" },
            new NavigationItem { Title = "引擎管理", Icon = "⚙️", ViewKey = "Engine" },
            new NavigationItem { Title = "模板管理", Icon = "📝", ViewKey = "Template" },
            new NavigationItem { Title = "设置", Icon = "⚙️", ViewKey = "Settings" }
        ];

        // 命令绑定
        NavigateCommand = new RelayCommand<NavigationItem>(OnNavigate);
        ToggleNavigationCommand = new RelayCommand(OnToggleNavigation);

        // 默认选中并导航到第一项
        OnNavigate(NavigationItems[0]);
    }

    /// <summary>导航项集合</summary>
    public ObservableCollection<NavigationItem> NavigationItems { get; }

    /// <summary>当前选中的导航项</summary>
    public NavigationItem? SelectedNavItem
    {
        get => _selectedNavItem;
        set => SetProperty(ref _selectedNavItem, value);
    }

    /// <summary>右侧内容区当前显示的视图</summary>
    public object? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    /// <summary>导航栏是否展开</summary>
    public bool IsNavigationExpanded
    {
        get => _isNavigationExpanded;
        set
        {
            if (SetProperty(ref _isNavigationExpanded, value))
                OnPropertyChanged(nameof(NavBarWidth));
        }
    }

    /// <summary>导航栏宽度（展开240，收起60）</summary>
    public double NavBarWidth => IsNavigationExpanded ? 240 : 60;

    /// <summary>导航切换命令</summary>
    public RelayCommand<NavigationItem> NavigateCommand { get; }

    /// <summary>导航栏展开/收起切换命令</summary>
    public RelayCommand ToggleNavigationCommand { get; }

    /// <summary>
    /// 导航切换逻辑：根据 ViewKey 创建对应 ViewModel 实例并设置为当前视图。
    /// </summary>
    private void OnNavigate(NavigationItem? item)
    {
        if (item == null) return;

        // 清除其他项的选中状态，只选中当前项
        foreach (var navItem in NavigationItems)
        {
            navItem.IsSelected = navItem == item;
        }

        SelectedNavItem = item;

        CurrentView = item.ViewKey switch
        {
            "ProjectList" => new ProjectListViewModel(),
            "Plugin" => new PluginViewModel(),
            "Engine" => new EngineViewModel(),
            "Template" => new TemplateViewModel(),
            "Settings" => new SettingsViewModel(),
            _ => CurrentView
        };
    }

    /// <summary>
    /// 切换导航栏展开/收起状态。
    /// </summary>
    private void OnToggleNavigation()
    {
        IsNavigationExpanded = !IsNavigationExpanded;
    }
}
