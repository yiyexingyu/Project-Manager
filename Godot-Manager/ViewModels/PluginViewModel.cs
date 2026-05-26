using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Godot_Manager.Helpers;
using Godot_Manager.Models;
using Godot_Manager.Services;

namespace Godot_Manager.ViewModels;

/// <summary>
/// 插件管理 ViewModel。
/// 管理三个标签页：本地插件、官方插件、收藏插件。
/// </summary>
public class PluginViewModel : ViewModelBase
{
    private readonly PluginScanService _scanService;
    private readonly OfficialPluginService _officialService;
    private readonly AppConfigService _configService;
    private readonly JsonStorageService<List<string>> _favoritesStorage;
    private readonly ToastService _toastService;

    private string _activeTab = "本地插件";
    private string _searchText = string.Empty;
    private bool _isLoading;
    private bool _isCacheValid;
    private DateTime _cacheTime;

    public PluginViewModel()
    {
        _scanService = new PluginScanService();
        _officialService = new OfficialPluginService();
        _configService = new AppConfigService();
        _favoritesStorage = new JsonStorageService<List<string>>("plugin_favorites.json");
        _toastService = new ToastService();

        ScanLocalCommand = new AsyncRelayCommand(ScanLocalPluginsAsync);
        RefreshOfficialCommand = new AsyncRelayCommand(RefreshOfficialAsync);
        EnablePluginCommand = new RelayCommand<LocalPlugin>(p => TogglePlugin(p, true));
        DisablePluginCommand = new RelayCommand<LocalPlugin>(p => TogglePlugin(p, false));
        OpenPluginDirCommand = new RelayCommand<LocalPlugin>(OpenPluginDir);
        DeletePluginCommand = new RelayCommand<LocalPlugin>(DeletePlugin);
        ToggleFavoriteCommand = new RelayCommand<string>(ToggleFavorite);
        SetDefaultPluginCommand = new RelayCommand<LocalPlugin>(SetDefaultPlugin);
        DownloadPluginCommand = new RelayCommand<OfficialPlugin>(DownloadPlugin);

        _ = InitializeAsync();
    }

    public string PageTitle => "插件管理";

    public string ActiveTab { get => _activeTab; set => SetProperty(ref _activeTab, value); }
    public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) FilterPlugins(); } }
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public bool IsCacheValid { get => _isCacheValid; set => SetProperty(ref _isCacheValid, value); }
    public string CacheStatus => IsCacheValid ? $"缓存有效（{_cacheTime:yyyy-MM-dd}）" : "缓存已过期";

    public ObservableCollection<LocalPlugin> LocalPlugins { get; } = [];
    public ObservableCollection<OfficialPlugin> OfficialPlugins { get; } = [];
    public ObservableCollection<LocalPlugin> FavoriteLocalPlugins { get; } = [];
    public ObservableCollection<OfficialPlugin> FavoriteOfficialPlugins { get; } = [];

    private List<OfficialPlugin> _allOfficialPlugins = [];
    private HashSet<string> _favoriteNames = [];

    public AsyncRelayCommand ScanLocalCommand { get; }
    public AsyncRelayCommand RefreshOfficialCommand { get; }
    public RelayCommand<LocalPlugin> EnablePluginCommand { get; }
    public RelayCommand<LocalPlugin> DisablePluginCommand { get; }
    public RelayCommand<LocalPlugin> OpenPluginDirCommand { get; }
    public RelayCommand<LocalPlugin> DeletePluginCommand { get; }
    public RelayCommand<string> ToggleFavoriteCommand { get; }
    public RelayCommand<LocalPlugin> SetDefaultPluginCommand { get; }
    public RelayCommand<OfficialPlugin> DownloadPluginCommand { get; }

    private async Task InitializeAsync()
    {
        await LoadFavoritesAsync();
        await ScanLocalPluginsAsync();
        await LoadOfficialFromCacheAsync();
    }

    private async Task ScanLocalPluginsAsync()
    {
        IsLoading = true;
        try
        {
            var config = await _configService.GetConfigAsync();
            var plugins = await _scanService.ScanPluginsAsync(config.PluginScanPaths);

            LocalPlugins.Clear();
            foreach (var p in plugins)
            {
                p.IsFavorite = _favoriteNames.Contains(p.Name);
                p.IsDefault = config.DefaultPluginNames.Contains(p.Name);
                LocalPlugins.Add(p);
            }
        }
        catch (Exception ex) { _toastService.ShowError($"扫描插件失败: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async Task LoadOfficialFromCacheAsync()
    {
        try
        {
            var cached = await _officialService.GetCachedPluginsAsync();
            var (valid, time) = await _officialService.GetCacheStatusAsync();
            _isCacheValid = valid;
            _cacheTime = time;
            OnPropertyChanged(nameof(CacheStatus));

            _allOfficialPlugins = cached;
            FilterPlugins();
        }
        catch { /* 缓存读取失败忽略 */ }
    }

    private async Task RefreshOfficialAsync()
    {
        IsLoading = true;
        try
        {
            await _officialService.RefreshCacheAsync();
            await LoadOfficialFromCacheAsync();
            _toastService.ShowSuccess("官方插件列表已更新");
        }
        catch (Exception ex) { _toastService.ShowError($"刷新失败: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async Task LoadFavoritesAsync()
    {
        _favoriteNames = [.. (await _favoritesStorage.LoadAsync())];
    }

    private async void ToggleFavorite(string? name)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (_favoriteNames.Contains(name))
            _favoriteNames.Remove(name);
        else
            _favoriteNames.Add(name);

        await _favoritesStorage.SaveAsync([.. _favoriteNames]);
        RefreshFavoriteViews();
    }

    private void TogglePlugin(LocalPlugin? p, bool enable)
    {
        if (p == null) return;
        p.IsEnabled = enable;
        _toastService.ShowInfo($"插件 '{p.Name}' 已{(enable ? "启用" : "禁用")}");
    }

    private void OpenPluginDir(LocalPlugin? p)
    {
        if (p == null) return;
        try { Process.Start("explorer.exe", p.InstallPath); }
        catch (Exception ex) { _toastService.ShowError($"打开目录失败: {ex.Message}"); }
    }

    private void DeletePlugin(LocalPlugin? p)
    {
        if (p == null) return;
        try
        {
            if (Directory.Exists(p.InstallPath))
                Directory.Delete(p.InstallPath, true);
            LocalPlugins.Remove(p);
            _toastService.ShowSuccess($"插件 '{p.Name}' 已删除");
        }
        catch (Exception ex) { _toastService.ShowError($"删除失败: {ex.Message}"); }
    }

    private async void SetDefaultPlugin(LocalPlugin? p)
    {
        if (p == null) return;
        p.IsDefault = !p.IsDefault;
        var config = await _configService.GetConfigAsync();
        if (p.IsDefault) config.DefaultPluginNames.Add(p.Name);
        else config.DefaultPluginNames.Remove(p.Name);
        await _configService.SaveConfigAsync();
    }

    private void DownloadPlugin(OfficialPlugin? p)
    {
        if (p == null) return;
        _toastService.ShowInfo($"下载功能将在后续版本实现: {p.Title}");
    }

    private void FilterPlugins()
    {
        OfficialPlugins.Clear();
        foreach (var p in _allOfficialPlugins)
        {
            p.IsFavorite = _favoriteNames.Contains(p.Title);
            if (string.IsNullOrEmpty(SearchText) ||
                p.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                OfficialPlugins.Add(p);
        }
        RefreshFavoriteViews();
    }

    private void RefreshFavoriteViews()
    {
        FavoriteLocalPlugins.Clear();
        foreach (var p in LocalPlugins.Where(p => _favoriteNames.Contains(p.Name)))
            FavoriteLocalPlugins.Add(p);

        FavoriteOfficialPlugins.Clear();
        foreach (var p in _allOfficialPlugins.Where(p => _favoriteNames.Contains(p.Title)))
            FavoriteOfficialPlugins.Add(p);
    }
}

