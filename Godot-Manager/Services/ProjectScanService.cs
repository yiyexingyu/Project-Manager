using System.Diagnostics;
using System.IO;
using Godot_Manager.Models;

namespace Godot_Manager.Services;

/// <summary>
/// 项目扫描服务。
/// 递归扫描配置的目录，识别 project.godot 文件并解析项目信息。
/// 基于 Godot 4.x 标准 INI 格式的 project.godot 文件。
/// </summary>
public class ProjectScanService
{
    /// <summary>
    /// 根据扫描路径配置扫描所有已启用的目录，返回发现的 Godot 项目列表。
    /// </summary>
    /// <param name="scanPaths">扫描路径配置列表</param>
    /// <returns>发现的 Godot 项目列表</returns>
    public Task<List<GodotProject>> ScanProjectsAsync(List<ScanPathConfig> scanPaths)
    {
        return Task.Run(() => ScanProjects(scanPaths));
    }

    private List<GodotProject> ScanProjects(List<ScanPathConfig> scanPaths)
    {
        var projects = new List<GodotProject>();
        var foundPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var scanPath in scanPaths)
        {
            if (!scanPath.IsEnabled || string.IsNullOrWhiteSpace(scanPath.Path))
                continue;

            if (!Directory.Exists(scanPath.Path))
                continue;

            try
            {
                // 递归搜索 project.godot 文件（限制深度 5 层避免性能问题）
                var godotFiles = Directory.GetFiles(scanPath.Path, "project.godot", SearchOption.AllDirectories)
                    .Take(500); // 安全上限

                foreach (var godotFile in godotFiles)
                {
                    var projectDir = Path.GetDirectoryName(godotFile);
                    if (projectDir == null || foundPaths.Contains(projectDir))
                        continue;

                    foundPaths.Add(projectDir);

                    var project = ParseProjectGodot(godotFile);
                    if (project != null)
                    {
                        projects.Add(project);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProjectScan] 扫描 {scanPath.Path} 失败: {ex.Message}");
            }
        }

        return projects;
    }

    /// <summary>
    /// 解析 project.godot 文件（Godot 4.x INI 格式），提取项目元信息。
    /// </summary>
    /// <param name="godotFilePath">project.godot 文件的完整路径</param>
    /// <returns>解析后的 GodotProject 对象，失败返回 null</returns>
    private static GodotProject? ParseProjectGodot(string godotFilePath)
    {
        try
        {
            var projectDir = Path.GetDirectoryName(godotFilePath) ?? string.Empty;
            var projectName = Path.GetFileName(projectDir);
            var engineVersion = string.Empty;
            var description = string.Empty;

            // 解析 INI 格式的 project.godot 文件
            var lines = File.ReadAllLines(godotFilePath);
            string? currentSection = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // 跳过空行和注释
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
                    continue;

                // 识别节 [section]
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    currentSection = trimmed[1..^1].Trim();
                    continue;
                }

                // 解析键值对 key=value
                var eqIndex = trimmed.IndexOf('=');
                if (eqIndex < 0) continue;

                var key = trimmed[..eqIndex].Trim();
                var value = trimmed[(eqIndex + 1)..].Trim().Trim('"');

                if (currentSection == "application")
                {
                    if (key == "config/name")
                        projectName = value;
                    else if (key == "config/description")
                        description = value;
                }
                else if (currentSection == null || currentSection == string.Empty)
                {
                    // Godot 4.x 顶层配置
                    if (key == "config_version")
                        engineVersion = value;
                }
            }

            // 从目录获取最后修改时间
            var lastModified = Directory.GetLastWriteTime(projectDir);

            return new GodotProject
            {
                Name = projectName,
                ProjectPath = projectDir,
                GodotFilePath = godotFilePath,
                EngineVersion = engineVersion,
                Description = description,
                LastModified = lastModified
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProjectScan] 解析 {godotFilePath} 失败: {ex.Message}");
            return null;
        }
    }
}
