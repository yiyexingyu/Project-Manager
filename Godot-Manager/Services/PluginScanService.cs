using System.Diagnostics;
using System.IO;
using Godot_Manager.Models;

namespace Godot_Manager.Services;

/// <summary>
/// 本地插件扫描服务。
/// 扫描用户配置的插件目录，识别 plugin.cfg 解析插件信息。
/// 基于 Godot 4.x 插件标准格式。
/// </summary>
public class PluginScanService
{
    /// <summary>
    /// 扫描所有配置的插件目录，返回发现的本地插件列表。
    /// </summary>
    public Task<List<LocalPlugin>> ScanPluginsAsync(List<ScanPathConfig> scanPaths)
    {
        return Task.Run(() => ScanPlugins(scanPaths));
    }

    private List<LocalPlugin> ScanPlugins(List<ScanPathConfig> scanPaths)
    {
        var plugins = new List<LocalPlugin>();
        var foundDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var scanPath in scanPaths)
        {
            if (!scanPath.IsEnabled || string.IsNullOrWhiteSpace(scanPath.Path))
                continue;

            if (!Directory.Exists(scanPath.Path))
                continue;

            try
            {
                // 扫描 plugin.cfg 文件（限制深度 3 层）
                var cfgFiles = Directory.GetFiles(scanPath.Path, "plugin.cfg", SearchOption.AllDirectories)
                    .Take(300);

                foreach (var cfgFile in cfgFiles)
                {
                    var dir = Path.GetDirectoryName(cfgFile);
                    if (dir == null || foundDirs.Contains(dir)) continue;

                    foundDirs.Add(dir);
                    var plugin = ParsePluginCfg(cfgFile);
                    if (plugin != null) plugins.Add(plugin);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PluginScan] 扫描 {scanPath.Path} 失败: {ex.Message}");
            }
        }

        return plugins;
    }

    /// <summary>
    /// 解析 plugin.cfg 文件（Godot 4.x INI 格式）。
    /// </summary>
    private static LocalPlugin? ParsePluginCfg(string cfgFilePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(cfgFilePath) ?? string.Empty;
            var name = Path.GetFileName(dir);
            var version = "unknown";
            var author = string.Empty;
            var description = string.Empty;
            var compatible = string.Empty;

            foreach (var line in File.ReadAllLines(cfgFilePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
                    continue;

                var eqIdx = trimmed.IndexOf('=');
                if (eqIdx < 0) continue;

                var key = trimmed[..eqIdx].Trim();
                var value = trimmed[(eqIdx + 1)..].Trim().Trim('"');

                switch (key)
                {
                    case "name": name = value; break;
                    case "version": version = value; break;
                    case "author": author = value; break;
                    case "description": description = value; break;
                }
            }

            return new LocalPlugin
            {
                Name = name,
                Version = version,
                InstallPath = dir,
                CompatibleEngineVersion = compatible,
                Author = author,
                Description = description
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PluginScan] 解析 {cfgFilePath} 失败: {ex.Message}");
            return null;
        }
    }
}
