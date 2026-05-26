using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Godot_Manager.Models;

namespace Godot_Manager.Services;

/// <summary>
/// 官方插件市场服务。
/// 对接 Godot Asset Library API，支持 7 天本地 JSON 缓存。
/// API 端点: https://godotengine.org/asset-library/api
/// </summary>
public class OfficialPluginService
{
    private readonly HttpClient _httpClient;
    private readonly JsonStorageService<PluginCacheData> _cacheStorage;
    private const string ApiBaseUrl = "https://godotengine.org/asset-library/api";

    public OfficialPluginService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GodotManager/1.0");
        _cacheStorage = new JsonStorageService<PluginCacheData>("plugin_cache.json");
    }

    /// <summary>
    /// 从缓存加载官方插件数据（7 天有效期）。
    /// </summary>
    public async Task<List<OfficialPlugin>> GetCachedPluginsAsync()
    {
        var cache = await _cacheStorage.LoadAsync();
        if (cache.IsValid && cache.Plugins.Count > 0)
            return cache.Plugins;

        return []; // 缓存过期或为空
    }

    /// <summary>
    /// 从 Godot 官方 API 拉取最新插件列表。
    /// </summary>
    /// <param name="search">搜索关键字</param>
    /// <param name="page">页码</param>
    /// <returns>插件列表</returns>
    public async Task<List<OfficialPlugin>> FetchPluginsAsync(string? search = null, int page = 0)
    {
        try
        {
            var url = $"{ApiBaseUrl}/assets?type=addon&max_results=50&offset={page * 50}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&filter={Uri.EscapeDataString(search)}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<AssetLibraryResponse>();
            return json?.Result?.Select(MapToPlugin).ToList() ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[OfficialPlugin] API 请求失败: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// 强制刷新缓存：拉取最新数据并保存。
    /// </summary>
    public async Task RefreshCacheAsync()
    {
        try
        {
            var plugins = await FetchPluginsAsync();
            var cache = new PluginCacheData
            {
                Plugins = plugins,
                CachedAt = DateTime.Now
            };
            await _cacheStorage.SaveAsync(cache);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[OfficialPlugin] 缓存刷新失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取缓存状态信息。
    /// </summary>
    public async Task<(bool isValid, DateTime cachedAt)> GetCacheStatusAsync()
    {
        var cache = await _cacheStorage.LoadAsync();
        return (cache.IsValid, cache.CachedAt);
    }

    /// <summary>
    /// 将 API 响应结果映射为 OfficialPlugin 模型。
    /// </summary>
    private static OfficialPlugin MapToPlugin(AssetResult item)
    {
        return new OfficialPlugin
        {
            AssetId = item.AssetId ?? string.Empty,
            Title = item.Title ?? string.Empty,
            Author = item.Author ?? string.Empty,
            Version = item.VersionString ?? string.Empty,
            Description = item.Description ?? string.Empty,
            IconUrl = item.IconUrl,
            DownloadUrl = item.DownloadUrl,
            GodotVersion = item.GodotVersion ?? string.Empty,
            Category = item.Category ?? string.Empty,
            Rating = item.Score
        };
    }

    // ==================== API JSON 响应模型 ====================

    private class AssetLibraryResponse
    {
        public List<AssetResult>? Result { get; set; }
        public int Page { get; set; }
        public int Pages { get; set; }
        public int Total { get; set; }
    }

    private class AssetResult
    {
        public string? AssetId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? VersionString { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public string? GodotVersion { get; set; }
        public string? Category { get; set; }
        public double Score { get; set; }
    }
}
