namespace Godot_Manager.Models;

/// <summary>
/// 本地插件数据模型。
/// 通过扫描本地插件目录中的 plugin.cfg 文件识别。
/// </summary>
public class LocalPlugin
{
    /// <summary>插件名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>插件版本号</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>插件本地安装路径</summary>
    public string InstallPath { get; set; } = string.Empty;

    /// <summary>适配的 Godot 引擎版本（如 "4.x"）</summary>
    public string CompatibleEngineVersion { get; set; } = string.Empty;

    /// <summary>是否已启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>是否已收藏</summary>
    public bool IsFavorite { get; set; } = false;

    /// <summary>是否为新建项目默认插件</summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>插件作者</summary>
    public string? Author { get; set; }

    /// <summary>插件简介</summary>
    public string? Description { get; set; }

    /// <summary>发现时间</summary>
    public DateTime DiscoveredAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 官方插件市场插件数据模型。
/// 对接 Godot Asset Library API 获取。
/// </summary>
public class OfficialPlugin
{
    /// <summary>插件 ID</summary>
    public string AssetId { get; set; } = string.Empty;

    /// <summary>插件名称</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>作者</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>版本</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>简介/描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>封面图 URL</summary>
    public string? IconUrl { get; set; }

    /// <summary>下载 URL</summary>
    public string? DownloadUrl { get; set; }

    /// <summary>适配引擎版本</summary>
    public string GodotVersion { get; set; } = string.Empty;

    /// <summary>分类标签</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>是否已收藏</summary>
    public bool IsFavorite { get; set; } = false;

    /// <summary>评分</summary>
    public double Rating { get; set; }
}

/// <summary>
/// 官方插件缓存数据结构。
/// </summary>
public class PluginCacheData
{
    /// <summary>缓存的插件列表</summary>
    public List<OfficialPlugin> Plugins { get; set; } = [];

    /// <summary>缓存时间戳</summary>
    public DateTime CachedAt { get; set; } = DateTime.Now;

    /// <summary>缓存是否有效（7天内）</summary>
    public bool IsValid => (DateTime.Now - CachedAt).TotalDays < 7;
}
