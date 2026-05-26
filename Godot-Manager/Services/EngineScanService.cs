using System.Diagnostics;
using System.IO;
using Godot_Manager.Models;

namespace Godot_Manager.Services;

/// <summary>
/// 引擎扫描服务。
/// 仅扫描用户配置的目录，无配置时不扫描（不生成默认路径）。
/// 区分 3.x/4.x、正式版/测试版。
/// </summary>
public class EngineScanService
{
    /// <summary>
    /// Godot 可执行文件可能的名称。
    /// </summary>
    private static readonly string[] ExeNames = ["godot.windows.opt.tools.64.exe", "godot.exe", "godot4.exe", "godot3.exe"];

    /// <summary>
    /// 扫描所有配置的目录，返回发现的 Godot 引擎列表。
    /// </summary>
    public Task<List<GodotEngine>> ScanEnginesAsync(List<ScanPathConfig> scanPaths)
    {
        return Task.Run(() => ScanEngines(scanPaths));
    }

    private List<GodotEngine> ScanEngines(List<ScanPathConfig> scanPaths)
    {
        var engines = new List<GodotEngine>();
        var foundPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var scanPath in scanPaths)
        {
            if (!scanPath.IsEnabled || string.IsNullOrWhiteSpace(scanPath.Path))
                continue;

            if (!Directory.Exists(scanPath.Path))
                continue;

            try
            {
                // 递归搜索 Godot 可执行文件
                foreach (var exeName in ExeNames)
                {
                    var exeFiles = Directory.GetFiles(scanPath.Path, exeName, SearchOption.AllDirectories)
                        .Take(100);

                    foreach (var exePath in exeFiles)
                    {
                        if (foundPaths.Contains(exePath)) continue;
                        foundPaths.Add(exePath);

                        var engine = ParseEngineInfo(exePath);
                        if (engine != null)
                            engines.Add(engine);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EngineScan] 扫描 {scanPath.Path} 失败: {ex.Message}");
            }
        }

        return engines;
    }

    /// <summary>
    /// 解析引擎信息：从可执行文件获取版本号，推断分支和发布类型。
    /// </summary>
    private static GodotEngine? ParseEngineInfo(string exePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(exePath) ?? string.Empty;
            var version = "未知";
            var branch = "未知";
            var releaseType = "正式版";

            // 尝试从可执行文件获取版本信息
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(3000);

                    // 解析类似于 "4.3.stable.official.xxxxx" 的输出
                    var parts = output.Trim().Split('.');
                    if (parts.Length >= 2)
                        version = $"{parts[0]}.{parts[1]}";
                    if (parts.Length >= 3 && parts[2].Contains("beta"))
                        releaseType = "测试版";
                    if (parts.Length > 0)
                        branch = parts[0].StartsWith('4') ? "4.x" : parts[0].StartsWith('3') ? "3.x" : "未知";
                }
            }
            catch
            {
                // 无法获取版本时使用目录名推断
                var dirName = Path.GetFileName(dir);
                if (dirName.Contains("4.")) branch = "4.x";
                else if (dirName.Contains("3.")) branch = "3.x";
            }

            return new GodotEngine
            {
                Version = version,
                ExecutablePath = exePath,
                InstallPath = dir,
                Branch = branch,
                ReleaseType = releaseType
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EngineScan] 解析引擎 {exePath} 失败: {ex.Message}");
            return null;
        }
    }
}
