using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Godot_Manager.Helpers;
using Godot_Manager.Models;
using Godot_Manager.Services;

namespace Godot_Manager.ViewModels;

/// <summary>
/// 项目管理列表 ViewModel。
/// 负责项目扫描、展示、搜索、排序、筛选以及快捷操作。
/// </summary>
public class ProjectListViewModel : ViewModelBase
{
    private readonly ProjectScanService _scanService;
    private readonly AppConfigService _configService;
    private readonly ToastService _toastService;

    private string _searchText = string.Empty;
    private string _sortBy = "名称";
    private string _filterEngineVersion = "全部";
    private string _viewMode = "列表";
    private bool _isLoading;
    private bool _isEmpty;

    public ProjectListViewModel()
    {
        _scanService = new ProjectScanService();
        _configService = new AppConfigService();
        _toastService = new ToastService();

        ScanProjectsCommand = new AsyncRelayCommand(ScanProjectsAsync);
        OpenProjectCommand = new RelayCommand<GodotProject>(OpenProject);
        OpenFolderCommand = new RelayCommand<GodotProject>(OpenFolder);
        RemoveFromListCommand = new RelayCommand<GodotProject>(RemoveFromList);
        ChangeEngineCommand = new RelayCommand<GodotProject>(ChangeEngine);
        ToggleViewModeCommand = new RelayCommand(ToggleViewMode);
        NewProjectCommand = new RelayCommand(OpenNewProjectDialog);
    }

    public string PageTitle => "项目管理";

    public ObservableCollection<GodotProject> Projects { get; } = [];

    private List<GodotProject> _filteredProjects = [];
    public List<GodotProject> FilteredProjects
    {
        get => _filteredProjects;
        private set => SetProperty(ref _filteredProjects, value);
    }

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyFilters(); }
    }

    public string SortBy
    {
        get => _sortBy;
        set { if (SetProperty(ref _sortBy, value)) ApplyFilters(); }
    }

    public string FilterEngineVersion
    {
        get => _filterEngineVersion;
        set { if (SetProperty(ref _filterEngineVersion, value)) ApplyFilters(); }
    }

    public string ViewMode
    {
        get => _viewMode;
        set => SetProperty(ref _viewMode, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    public List<string> SortOptions { get; } = ["名称", "时间", "引擎版本"];
    public ObservableCollection<string> EngineVersionOptions { get; } = ["全部"];

    public AsyncRelayCommand ScanProjectsCommand { get; }
    public RelayCommand<GodotProject> OpenProjectCommand { get; }
    public RelayCommand<GodotProject> OpenFolderCommand { get; }
    public RelayCommand<GodotProject> RemoveFromListCommand { get; }
    public RelayCommand<GodotProject> ChangeEngineCommand { get; }
    public RelayCommand ToggleViewModeCommand { get; }
    public RelayCommand NewProjectCommand { get; }

    public event Action? NewProjectRequested;

    public async Task ScanProjectsAsync()
    {
        IsLoading = true;
        try
        {
            var config = await _configService.GetConfigAsync();
            var projects = await _scanService.ScanProjectsAsync(config.ProjectScanPaths);

            Projects.Clear();
            foreach (var p in projects) Projects.Add(p);

            EngineVersionOptions.Clear();
            EngineVersionOptions.Add("全部");
            foreach (var ver in projects.Select(p => p.EngineVersion).Where(v => !string.IsNullOrEmpty(v)).Distinct().OrderBy(v => v))
                EngineVersionOptions.Add(ver);

            ApplyFilters();
            _toastService.ShowSuccess($"扫描完成，发现 {projects.Count} 个项目");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"扫描失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenProject(GodotProject? project)
    {
        if (project == null) return;
        try
        {
            var enginePath = project.AssociatedEnginePath;
            if (string.IsNullOrEmpty(enginePath) || !File.Exists(enginePath))
            {
                _toastService.ShowWarning("未找到关联的 Godot 引擎，请先设置引擎路径");
                return;
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = enginePath,
                Arguments = $"--path \"{project.ProjectPath}\"",
                UseShellExecute = false
            });
            _toastService.ShowInfo($"正在打开项目: {project.Name}");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"打开项目失败: {ex.Message}");
        }
    }

    private void OpenFolder(GodotProject? project)
    {
        if (project == null) return;
        try { Process.Start("explorer.exe", project.ProjectPath); }
        catch (Exception ex) { _toastService.ShowError($"打开目录失败: {ex.Message}"); }
    }

    private void RemoveFromList(GodotProject? project)
    {
        if (project == null) return;
        Projects.Remove(project);
        ApplyFilters();
        _toastService.ShowInfo($"已从列表移除: {project.Name}");
    }

    private void ChangeEngine(GodotProject? project)
    {
        if (project == null) return;
        _toastService.ShowInfo("修改关联引擎功能将在引擎管理模块完成后启用");
    }

    private void ToggleViewMode()
    {
        ViewMode = ViewMode == "列表" ? "卡片" : "列表";
    }

    private void OpenNewProjectDialog()
    {
        NewProjectRequested?.Invoke();
    }

    private void ApplyFilters()
    {
        var query = Projects.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var kw = SearchText.Trim();
            query = query.Where(p => p.Name.Contains(kw, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(FilterEngineVersion) && FilterEngineVersion != "全部")
            query = query.Where(p => p.EngineVersion == FilterEngineVersion);

        query = SortBy switch
        {
            "时间" => query.OrderByDescending(p => p.LastModified),
            "引擎版本" => query.OrderByDescending(p => p.EngineVersion),
            _ => query.OrderBy(p => p.Name)
        };

        FilteredProjects = query.ToList();
        IsEmpty = FilteredProjects.Count == 0;
    }
}

