using System.Collections.ObjectModel;
using System.IO;
using Godot_Manager.Helpers;
using Godot_Manager.Services;

namespace Godot_Manager.ViewModels;

/// <summary>
/// 新建项目弹窗 ViewModel。
/// 负责表单校验、默认配置联动、项目创建逻辑。
/// </summary>
public class NewProjectViewModel : ViewModelBase
{
    private readonly ProjectService _projectService;
    private readonly AppConfigService _configService;
    private readonly ToastService _toastService;

    private string _projectName = string.Empty;
    private string _projectPath = string.Empty;
    private string? _selectedEngineVersion;
    private string? _selectedTemplate;
    private string? _selectedScriptTemplate;
    private string? _projectNameError;
    private string? _projectPathError;
    private bool _isCreating;

    public NewProjectViewModel(
        ProjectService projectService,
        AppConfigService configService,
        ToastService toastService)
    {
        _projectService = projectService;
        _configService = configService;
        _toastService = toastService;

        CreateProjectCommand = new AsyncRelayCommand(CreateProjectAsync, () => !HasErrors);
        BrowsePathCommand = new RelayCommand(BrowsePath);
        CancelCommand = new RelayCommand(() => CloseRequested?.Invoke());

        // 异步加载默认配置
        _ = LoadDefaultsAsync();
    }

    // ==================== 属性 ====================

    public string ProjectName
    {
        get => _projectName;
        set
        {
            if (SetProperty(ref _projectName, value))
                ValidateProjectName();
        }
    }

    public string ProjectPath
    {
        get => _projectPath;
        set
        {
            if (SetProperty(ref _projectPath, value))
                ValidateProjectPath();
        }
    }

    public string? SelectedEngineVersion
    {
        get => _selectedEngineVersion;
        set => SetProperty(ref _selectedEngineVersion, value);
    }

    public string? SelectedTemplate
    {
        get => _selectedTemplate;
        set => SetProperty(ref _selectedTemplate, value);
    }

    public string? SelectedScriptTemplate
    {
        get => _selectedScriptTemplate;
        set => SetProperty(ref _selectedScriptTemplate, value);
    }

    public string? ProjectNameError
    {
        get => _projectNameError;
        set => SetProperty(ref _projectNameError, value);
    }

    public string? ProjectPathError
    {
        get => _projectPathError;
        set => SetProperty(ref _projectPathError, value);
    }

    public bool IsCreating
    {
        get => _isCreating;
        set => SetProperty(ref _isCreating, value);
    }

    public bool HasErrors => !string.IsNullOrEmpty(ProjectNameError) || !string.IsNullOrEmpty(ProjectPathError);

    /// <summary>可选引擎版本列表</summary>
    public ObservableCollection<string> EngineVersions { get; } = [];

    /// <summary>可选项目模板列表</summary>
    public ObservableCollection<string> ProjectTemplates { get; } = ["无"];

    /// <summary>可选脚本模板列表</summary>
    public ObservableCollection<string> ScriptTemplates { get; } = ["无"];

    /// <summary>选中的插件名称列表</summary>
    public ObservableCollection<PluginSelectionItem> PluginItems { get; } = [];

    // ==================== 命令 ====================

    public AsyncRelayCommand CreateProjectCommand { get; }
    public RelayCommand BrowsePathCommand { get; }
    public RelayCommand CancelCommand { get; }

    /// <summary>关闭弹窗请求事件</summary>
    public event Action? CloseRequested;

    /// <summary>项目创建成功事件</summary>
    public event Action? ProjectCreated;

    // ==================== 方法 ====================

    /// <summary>
    /// 加载默认配置（默认引擎、默认插件、默认模板）。
    /// </summary>
    private async Task LoadDefaultsAsync()
    {
        try
        {
            var config = await _configService.GetConfigAsync();

            // 加载默认引擎版本
            if (!string.IsNullOrEmpty(config.DefaultEngineVersion))
                SelectedEngineVersion = config.DefaultEngineVersion;

            // 加载默认模板
            if (!string.IsNullOrEmpty(config.DefaultProjectTemplateName))
                SelectedTemplate = config.DefaultProjectTemplateName;

            if (!string.IsNullOrEmpty(config.DefaultScriptTemplateName))
                SelectedScriptTemplate = config.DefaultScriptTemplateName;

            // TODO: 从引擎/插件/模板服务加载实际可选列表
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NewProject] 加载默认配置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 校验项目名称。
    /// </summary>
    private void ValidateProjectName()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ProjectNameError = "项目名称不能为空";
        }
        else if (ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            ProjectNameError = "项目名称包含非法字符";
        }
        else
        {
            ProjectNameError = null;
        }
    }

    /// <summary>
    /// 校验存储路径。
    /// </summary>
    private void ValidateProjectPath()
    {
        if (string.IsNullOrWhiteSpace(ProjectPath))
        {
            ProjectPathError = "存储路径不能为空";
        }
        else if (!Directory.Exists(ProjectPath))
        {
            ProjectPathError = "存储路径不存在";
        }
        else
        {
            ProjectPathError = null;
        }
    }

    /// <summary>
    /// 浏览文件夹选择存储路径（使用 WPF 原生 OpenFolderDialog）。
    /// </summary>
    private void BrowsePath()
    {
        var selectedPath = Helpers.WpfFolderBrowser.ShowDialog("选择项目存储路径", ProjectPath);
        if (!string.IsNullOrEmpty(selectedPath))
        {
            ProjectPath = selectedPath;
        }
    }

    /// <summary>
    /// 执行项目创建。
    /// </summary>
    private async Task CreateProjectAsync()
    {
        ValidateProjectName();
        ValidateProjectPath();

        if (HasErrors)
        {
            _toastService.ShowWarning("请修正表单中的错误后再创建");
            return;
        }

        IsCreating = true;
        try
        {
            var config = new ProjectService.NewProjectConfig
            {
                ProjectName = ProjectName.Trim(),
                ProjectPath = ProjectPath.Trim(),
                EngineVersion = SelectedEngineVersion ?? string.Empty,
                SelectedPluginNames = PluginItems.Where(p => p.IsSelected).Select(p => p.Name).ToList(),
                SelectedTemplateName = SelectedTemplate == "无" ? null : SelectedTemplate,
                SelectedScriptTemplateName = SelectedScriptTemplate == "无" ? null : SelectedScriptTemplate
            };

            var success = await _projectService.CreateProjectAsync(config);
            if (success)
            {
                ProjectCreated?.Invoke();
                CloseRequested?.Invoke();
            }
        }
        finally
        {
            IsCreating = false;
        }
    }
}

/// <summary>
/// 新建项目弹窗中的插件选择项模型。
/// </summary>
public class PluginSelectionItem : ViewModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
