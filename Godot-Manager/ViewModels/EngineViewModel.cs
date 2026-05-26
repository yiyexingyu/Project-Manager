using System.Collections.ObjectModel;
using System.IO;
using Godot_Manager.Helpers;
using Godot_Manager.Models;
using Godot_Manager.Services;

namespace Godot_Manager.ViewModels;

/// <summary>
/// 引擎管理 ViewModel。
/// 负责引擎扫描、展示、默认引擎设置、路径配置。
/// </summary>
public class EngineViewModel : ViewModelBase
{
    private readonly EngineScanService _scanService;
    private readonly AppConfigService _configService;
    private readonly ToastService _toastService;
    private bool _isLoading;
    private string _newScanPath = string.Empty;

    public EngineViewModel()
    {
        _scanService = new EngineScanService();
        _configService = new AppConfigService();
        _toastService = new ToastService();

        ScanEnginesCommand = new AsyncRelayCommand(ScanEnginesAsync);
        SetDefaultCommand = new RelayCommand<GodotEngine>(SetDefaultEngine);
        AddScanPathCommand = new AsyncRelayCommand(AddScanPathAsync);
        RemoveScanPathCommand = new RelayCommand<ScanPathConfig>(RemoveScanPath);
        BrowsePathCommand = new RelayCommand(BrowsePath);
        DownloadEngineCommand = new RelayCommand(() =>
            _toastService.ShowInfo("引擎在线下载功能即将推出"));

        _ = ScanEnginesAsync();
    }

    public string PageTitle => "引擎管理";
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public string NewScanPath { get => _newScanPath; set => SetProperty(ref _newScanPath, value); }

    public ObservableCollection<GodotEngine> Engines { get; } = [];
    public ObservableCollection<ScanPathConfig> ScanPaths { get; } = [];

    public AsyncRelayCommand ScanEnginesCommand { get; }
    public RelayCommand<GodotEngine> SetDefaultCommand { get; }
    public AsyncRelayCommand AddScanPathCommand { get; }
    public RelayCommand<ScanPathConfig> RemoveScanPathCommand { get; }
    public RelayCommand BrowsePathCommand { get; }
    public RelayCommand DownloadEngineCommand { get; }

    private async Task ScanEnginesAsync()
    {
        IsLoading = true;
        try
        {
            var config = await _configService.GetConfigAsync();
            ScanPaths.Clear();
            foreach (var p in config.EngineScanPaths) ScanPaths.Add(p);

            var engines = await _scanService.ScanEnginesAsync(config.EngineScanPaths);
            Engines.Clear();
            foreach (var e in engines)
            {
                e.IsDefault = config.DefaultEngineVersion == e.Version;
                Engines.Add(e);
            }
        }
        catch (Exception ex) { _toastService.ShowError($"扫描失败: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async void SetDefaultEngine(GodotEngine? engine)
    {
        if (engine == null) return;
        var config = await _configService.GetConfigAsync();
        config.DefaultEngineVersion = engine.Version;
        await _configService.SaveConfigAsync();

        foreach (var e in Engines) e.IsDefault = e.Version == engine.Version;
        _toastService.ShowSuccess($"已设为默认引擎: {engine.Version}");
    }

    private async Task AddScanPathAsync()
    {
        if (string.IsNullOrWhiteSpace(NewScanPath)) return;
        if (!Directory.Exists(NewScanPath)) { _toastService.ShowWarning("路径不存在"); return; }

        var config = await _configService.GetConfigAsync();
        if (config.EngineScanPaths.Any(p => p.Path == NewScanPath)) { _toastService.ShowWarning("路径已存在"); return; }

        config.EngineScanPaths.Add(new ScanPathConfig { Path = NewScanPath });
        await _configService.SaveConfigAsync();
        ScanPaths.Add(new ScanPathConfig { Path = NewScanPath });
        NewScanPath = string.Empty;
        await ScanEnginesAsync();
    }

    private async void RemoveScanPath(ScanPathConfig? path)
    {
        if (path == null) return;
        var config = await _configService.GetConfigAsync();
        config.EngineScanPaths.RemoveAll(p => p.Path == path.Path);
        await _configService.SaveConfigAsync();
        ScanPaths.Remove(path);
    }

    private void BrowsePath()
    {
        var selected = Helpers.WpfFolderBrowser.ShowDialog("选择引擎扫描目录");
        if (!string.IsNullOrEmpty(selected)) NewScanPath = selected;
    }
}

