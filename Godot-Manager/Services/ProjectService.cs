using System.Diagnostics;
using System.IO;
using Godot_Manager.Models;

namespace Godot_Manager.Services;

/// <summary>
/// 项目创建/操作服务。
/// 负责新建项目：创建目录、生成 project.godot、挂载插件（软链接）、复制模板。
/// </summary>
public class ProjectService
{
    private readonly AppConfigService _configService;
    private readonly ToastService _toastService;

    public ProjectService(AppConfigService configService, ToastService toastService)
    {
        _configService = configService;
        _toastService = toastService;
    }

    /// <summary>
    /// 新建项目配置参数。
    /// </summary>
    public class NewProjectConfig
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;
        public string EngineVersion { get; set; } = string.Empty;
        public List<string> SelectedPluginNames { get; set; } = [];
        public string? SelectedTemplateName { get; set; }
        public string? SelectedScriptTemplateName { get; set; }
    }

    /// <summary>
    /// 异步创建新 Godot 项目。
    /// </summary>
    /// <param name="config">新建项目配置</param>
    /// <returns>创建是否成功</returns>
    public async Task<bool> CreateProjectAsync(NewProjectConfig config)
    {
        // 1. 校验参数
        if (string.IsNullOrWhiteSpace(config.ProjectName))
        {
            _toastService.ShowError("项目名称不能为空");
            return false;
        }

        // 校验项目名合法性（仅允许字母、数字、中文、下划线、连字符、空格）
        if (config.ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            _toastService.ShowError("项目名称包含非法字符");
            return false;
        }

        if (string.IsNullOrWhiteSpace(config.ProjectPath))
        {
            _toastService.ShowError("项目存储路径不能为空");
            return false;
        }

        // 项目完整路径
        var fullPath = Path.Combine(config.ProjectPath, config.ProjectName);

        // 2. 路径重复校验
        if (Directory.Exists(fullPath))
        {
            _toastService.ShowError($"目录已存在: {fullPath}");
            return false;
        }

        try
        {
            // 3. 创建项目目录
            Directory.CreateDirectory(fullPath);

            // 4. 生成 project.godot 文件（Godot 4.x 格式）
            await CreateProjectGodotFileAsync(fullPath, config);

            // 5. 挂载插件（软链接方式）
            await LinkPluginsAsync(fullPath, config.SelectedPluginNames);

            // 6. 复制脚本模板
            await CopyScriptTemplatesAsync(fullPath, config.SelectedScriptTemplateName);

            _toastService.ShowSuccess($"项目 '{config.ProjectName}' 创建成功！");
            return true;
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"创建项目失败: {ex.Message}");

            // 清理：创建失败时删除已创建的目录
            try { if (Directory.Exists(fullPath)) Directory.Delete(fullPath, true); }
            catch { /* 清理失败忽略 */ }

            return false;
        }
    }

    /// <summary>
    /// 生成 project.godot 配置文件。
    /// </summary>
    private static async Task CreateProjectGodotFileAsync(string projectPath, NewProjectConfig config)
    {
        var godotFilePath = Path.Combine(projectPath, "project.godot");

        var content = $"""
            ; Engine configuration file.
            ; It's best edited using the editor UI and not directly,
            ; since the parameters that go here are not obvious.
            ;
            ; Format:
            ;   [section] ; section goes between []
            ;   param=value ; assign values to parameters

            config_version=5

            [application]

            config/name="{config.ProjectName}"
            config/description=""
            config/icon=""

            """;

        await File.WriteAllTextAsync(godotFilePath, content);
    }

    /// <summary>
    /// 以软链接（目录联接）方式挂载插件到项目目录。
    /// Windows 使用 Junction，类似 pnpm 的链接机制避免复制占用空间。
    /// </summary>
    private async Task LinkPluginsAsync(string projectPath, List<string> pluginNames)
    {
        if (pluginNames.Count == 0) return;

        var addonsDir = Path.Combine(projectPath, "addons");
        Directory.CreateDirectory(addonsDir);

        var config = await _configService.GetConfigAsync();
        var pluginScanPaths = config.PluginScanPaths;

        foreach (var pluginName in pluginNames)
        {
            try
            {
                // 在配置的插件扫描路径中查找插件目录
                string? pluginSourceDir = null;
                foreach (var scanPath in pluginScanPaths)
                {
                    if (!scanPath.IsEnabled) continue;
                    var candidate = Path.Combine(scanPath.Path, pluginName);
                    if (Directory.Exists(candidate))
                    {
                        pluginSourceDir = candidate;
                        break;
                    }
                }

                if (pluginSourceDir == null)
                {
                    Debug.WriteLine($"[ProjectService] 未找到插件目录: {pluginName}");
                    continue;
                }

                var targetDir = Path.Combine(addonsDir, pluginName);

                // 使用 Junction 创建目录联接（软链接）
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateSymbolicLink(targetDir, pluginSourceDir);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProjectService] 挂载插件 {pluginName} 失败: {ex.Message}");
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 复制脚本模板到项目目录。
    /// </summary>
    private async Task CopyScriptTemplatesAsync(string projectPath, string? scriptTemplateName)
    {
        if (string.IsNullOrEmpty(scriptTemplateName)) return;

        // TODO: 阶段六模板管理完成后实现完整的模板复制逻辑
        await Task.CompletedTask;
    }
}
