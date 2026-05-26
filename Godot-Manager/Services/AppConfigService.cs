namespace Godot_Manager.Services;

/// <summary>
/// 应用全局配置数据模型。
/// 存储用户偏好设置：扫描路径、默认引擎/插件/模板、界面偏好等。
/// </summary>
public class AppConfig
{
    /// <summary>项目扫描目录配置列表</summary>
    public List<ScanPathConfig> ProjectScanPaths { get; set; } = [];

    /// <summary>引擎扫描目录配置列表</summary>
    public List<ScanPathConfig> EngineScanPaths { get; set; } = [];

    /// <summary>本地插件扫描目录配置列表</summary>
    public List<ScanPathConfig> PluginScanPaths { get; set; } = [];

    /// <summary>默认启动引擎版本号（null 表示未设置）</summary>
    public string? DefaultEngineVersion { get; set; }

    /// <summary>默认新建项目插件名称列表</summary>
    public List<string> DefaultPluginNames { get; set; } = [];

    /// <summary>默认新建项目模板名称（null 表示未设置）</summary>
    public string? DefaultProjectTemplateName { get; set; }

    /// <summary>默认新建脚本模板名称（null 表示未设置）</summary>
    public string? DefaultScriptTemplateName { get; set; }

    /// <summary>默认项目列表视图模式：List / Card</summary>
    public string ProjectListViewMode { get; set; } = "List";

    /// <summary>导航栏是否收起</summary>
    public bool IsNavigationCollapsed { get; set; } = false;

    /// <summary>窗口宽度</summary>
    public double WindowWidth { get; set; } = 1200;

    /// <summary>窗口高度</summary>
    public double WindowHeight { get; set; } = 750;
}

/// <summary>
/// 扫描路径配置项。
/// </summary>
public class ScanPathConfig
{
    /// <summary>目录路径</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>是否启用此扫描路径</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>添加时间</summary>
    public DateTime AddedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 应用全局配置管理服务。
/// 负责 AppConfig 的加载、保存和运行时访问。
/// </summary>
public class AppConfigService
{
    private readonly JsonStorageService<AppConfig> _storage;
    private AppConfig? _config;

    public AppConfigService()
    {
        _storage = new JsonStorageService<AppConfig>("app_config.json");
    }

    /// <summary>
    /// 获取当前配置（懒加载，首次访问时自动从文件读取）。
    /// </summary>
    public async Task<AppConfig> GetConfigAsync()
    {
        return _config ??= await _storage.LoadAsync();
    }

    /// <summary>
    /// 保存当前配置到本地 JSON 文件。
    /// </summary>
    public async Task SaveConfigAsync()
    {
        if (_config != null)
            await _storage.SaveAsync(_config);
    }

    /// <summary>
    /// 刷新配置（重新从文件加载，覆盖内存中的缓存）。
    /// </summary>
    public async Task RefreshAsync()
    {
        _config = await _storage.LoadAsync();
    }
}
