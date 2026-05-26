using System.Collections.ObjectModel;
using Godot_Manager.Helpers;
using Godot_Manager.Services;

namespace Godot_Manager.ViewModels;

/// <summary>
/// 设置页 ViewModel。
/// 提供项目扫描目录、插件扫描目录、引擎扫描目录等可配置项的管理。
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly AppConfigService _configService;
    private readonly ToastService _toastService;

    private string _newProjectPath = string.Empty;
    private string _newPluginPath = string.Empty;
    private string _newEnginePath = string.Empty;

    public SettingsViewModel()
    {
        _configService = new AppConfigService();
        _toastService = new ToastService();

        AddProjectPathCommand = new RelayCommand(async () => await AddProjectPathAsync());
        RemoveProjectPathCommand = new RelayCommand<ScanPathConfig>(RemoveProjectPath);
        ToggleProjectPathCommand = new RelayCommand<ScanPathConfig>(ToggleProjectPath);

        AddPluginPathCommand = new RelayCommand(async () => await AddPluginPathAsync());
        RemovePluginPathCommand = new RelayCommand<ScanPathConfig>(RemovePluginPath);
        TogglePluginPathCommand = new RelayCommand<ScanPathConfig>(TogglePluginPath);

        AddEnginePathCommand = new RelayCommand(async () => await AddEnginePathAsync());
        RemoveEnginePathCommand = new RelayCommand<ScanPathConfig>(RemoveEnginePath);
        ToggleEnginePathCommand = new RelayCommand<ScanPathConfig>(ToggleEnginePath);

        _ = InitializeAsync();
    }

    /// <summary>项目扫描目录列表</summary>
    public ObservableCollection<ScanPathConfig> ProjectScanPaths { get; } = [];

    /// <summary>插件扫描目录列表</summary>
    public ObservableCollection<ScanPathConfig> PluginScanPaths { get; } = [];

    /// <summary>引擎扫描目录列表</summary>
    public ObservableCollection<ScanPathConfig> EngineScanPaths { get; } = [];

    public string NewProjectPath
    {
        get => _newProjectPath;
        set => SetProperty(ref _newProjectPath, value);
    }

    public string NewPluginPath
    {
        get => _newPluginPath;
        set => SetProperty(ref _newPluginPath, value);
    }

    public string NewEnginePath
    {
        get => _newEnginePath;
        set => SetProperty(ref _newEnginePath, value);
    }

    public RelayCommand AddProjectPathCommand { get; }
    public RelayCommand<ScanPathConfig> RemoveProjectPathCommand { get; }
    public RelayCommand<ScanPathConfig> ToggleProjectPathCommand { get; }

    public RelayCommand AddPluginPathCommand { get; }
    public RelayCommand<ScanPathConfig> RemovePluginPathCommand { get; }
    public RelayCommand<ScanPathConfig> TogglePluginPathCommand { get; }

    public RelayCommand AddEnginePathCommand { get; }
    public RelayCommand<ScanPathConfig> RemoveEnginePathCommand { get; }
    public RelayCommand<ScanPathConfig> ToggleEnginePathCommand { get; }

    private async Task InitializeAsync()
    {
        try
        {
            var config = await _configService.GetConfigAsync();

            ProjectScanPaths.Clear();
            foreach (var p in config.ProjectScanPaths) ProjectScanPaths.Add(p);

            PluginScanPaths.Clear();
            foreach (var p in config.PluginScanPaths) PluginScanPaths.Add(p);

            EngineScanPaths.Clear();
            foreach (var p in config.EngineScanPaths) EngineScanPaths.Add(p);
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"加载配置失败：{ex.Message}");
        }
    }

    private async Task AddProjectPathAsync()
    {
        try
        {
            var path = WpfFolderBrowser.ShowDialog("选择项目扫描目录");
            if (string.IsNullOrWhiteSpace(path)) return;

            var config = await _configService.GetConfigAsync();
            if (config.ProjectScanPaths.Any(p => p.Path == path))
            {
                _toastService.ShowWarning("路径已存在");
                return;
            }

            var item = new ScanPathConfig { Path = path, IsEnabled = true };
            config.ProjectScanPaths.Add(item);
            ProjectScanPaths.Add(item);
            await _configService.SaveConfigAsync();
            _toastService.ShowSuccess("已添加项目扫描路径");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"添加失败：{ex.Message}");
        }
    }

    private async void RemoveProjectPath(ScanPathConfig? path)
    {
        if (path == null) return;
        try
        {
            var config = await _configService.GetConfigAsync();
            config.ProjectScanPaths.RemoveAll(p => p.Path == path.Path);
            ProjectScanPaths.Remove(path);
            await _configService.SaveConfigAsync();
            _toastService.ShowSuccess("已移除扫描路径");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"移除失败：{ex.Message}");
        }
    }

    private async void ToggleProjectPath(ScanPathConfig? path)
    {
        if (path == null) return;
        try
        {
            path.IsEnabled = !path.IsEnabled;
            var config = await _configService.GetConfigAsync();
            var match = config.ProjectScanPaths.FirstOrDefault(p => p.Path == path.Path);
            if (match != null) match.IsEnabled = path.IsEnabled;
            await _configService.SaveConfigAsync();
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"操作失败：{ex.Message}");
        }
    }

    private async Task AddPluginPathAsync()
    {
        try
        {
            var path = Helpers.WpfFolderBrowser.ShowDialog("选择插件扫描目录");
            if (string.IsNullOrWhiteSpace(path)) return;

            var config = await _configService.GetConfigAsync();
            if (config.PluginScanPaths.Any(p => p.Path == path))
            {
                _toastService.ShowWarning("路径已存在");
                return;
            }

            var item = new ScanPathConfig { Path = path, IsEnabled = true };
            config.PluginScanPaths.Add(item);
            PluginScanPaths.Add(item);
            await _configService.SaveConfigAsync();
            _toastService.ShowSuccess("已添加插件扫描路径");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"添加失败：{ex.Message}");
        }
    }

    private async void RemovePluginPath(ScanPathConfig? path)
    {
        if (path == null) return;
        try
        {
            var config = await _configService.GetConfigAsync();
            config.PluginScanPaths.RemoveAll(p => p.Path == path.Path);
            PluginScanPaths.Remove(path);
            await _configService.SaveConfigAsync();
            _toastService.ShowSuccess("已移除扫描路径");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"移除失败：{ex.Message}");
        }
    }

    private async void TogglePluginPath(ScanPathConfig? path)
    {
        if (path == null) return;
        try
        {
            path.IsEnabled = !path.IsEnabled;
            var config = await _configService.GetConfigAsync();
            var match = config.PluginScanPaths.FirstOrDefault(p => p.Path == path.Path);
            if (match != null) match.IsEnabled = path.IsEnabled;
            await _configService.SaveConfigAsync();
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"操作失败：{ex.Message}");
        }
    }

    private async Task AddEnginePathAsync()
    {
        try
        {
            var path = Helpers.WpfFolderBrowser.ShowDialog("选择引擎扫描目录");
            if (string.IsNullOrWhiteSpace(path)) return;

            var config = await _configService.GetConfigAsync();
            if (config.EngineScanPaths.Any(p => p.Path == path))
            {
                _toastService.ShowWarning("路径已存在");
                return;
            }

            var item = new ScanPathConfig { Path = path, IsEnabled = true };
            config.EngineScanPaths.Add(item);
            EngineScanPaths.Add(item);
            await _configService.SaveConfigAsync();
            _toastService.ShowSuccess("已添加引擎扫描路径");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"添加失败：{ex.Message}");
        }
    }

    private async void RemoveEnginePath(ScanPathConfig? path)
    {
        if (path == null) return;
        try
        {
            var config = await _configService.GetConfigAsync();
            config.EngineScanPaths.RemoveAll(p => p.Path == path.Path);
            EngineScanPaths.Remove(path);
            await _configService.SaveConfigAsync();
            _toastService.ShowSuccess("已移除扫描路径");
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"移除失败：{ex.Message}");
        }
    }

    private async void ToggleEnginePath(ScanPathConfig? path)
    {
        if (path == null) return;
        try
        {
            path.IsEnabled = !path.IsEnabled;
            var config = await _configService.GetConfigAsync();
            var match = config.EngineScanPaths.FirstOrDefault(p => p.Path == path.Path);
            if (match != null) match.IsEnabled = path.IsEnabled;
            await _configService.SaveConfigAsync();
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"操作失败：{ex.Message}");
        }
    }
}
