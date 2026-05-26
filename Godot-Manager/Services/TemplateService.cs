using System.IO;
using Godot_Manager.Models;

namespace Godot_Manager.Services;

/// <summary>
/// 模板管理服务。
/// 负责项目模板和脚本模板的扫描、CRUD 操作。
/// </summary>
public class TemplateService
{
    private readonly string _templateBaseDir;

    public TemplateService()
    {
        _templateBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Templates");
        Directory.CreateDirectory(Path.Combine(_templateBaseDir, "Projects"));
        Directory.CreateDirectory(Path.Combine(_templateBaseDir, "Scripts"));
    }

    /// <summary>
    /// 扫描所有模板，返回项目模板和脚本模板列表。
    /// </summary>
    public Task<(List<ProjectTemplate> projectTemplates, List<ScriptTemplate> scriptTemplates)> ScanTemplatesAsync()
    {
        return Task.Run(() =>
        {
            var projects = ScanProjectTemplates();
            var scripts = ScanScriptTemplates();
            return (projects, scripts);
        });
    }

    private List<ProjectTemplate> ScanProjectTemplates()
    {
        var templates = new List<ProjectTemplate>();
        var projectDir = Path.Combine(_templateBaseDir, "Projects");

        if (!Directory.Exists(projectDir)) return templates;

        foreach (var dir in Directory.GetDirectories(projectDir))
        {
            var name = Path.GetFileName(dir);
            templates.Add(new ProjectTemplate
            {
                Name = name,
                DirectoryPath = dir,
                Description = "用户项目模板",
                CompatibleEngineVersion = "4.x",
                CreatedAt = Directory.GetCreationTime(dir)
            });
        }
        return templates;
    }

    private List<ScriptTemplate> ScanScriptTemplates()
    {
        var templates = new List<ScriptTemplate>();
        var scriptsDir = Path.Combine(_templateBaseDir, "Scripts");

        if (!Directory.Exists(scriptsDir)) return templates;

        foreach (var file in Directory.GetFiles(scriptsDir, "*.*"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var ext = Path.GetExtension(file);
            var language = ext switch
            {
                ".gd" => "GDScript",
                ".cs" => "C#",
                _ => "Other"
            };

            string codePreview;
            try
            {
                var content = File.ReadAllText(file);
                codePreview = content.Length > 500 ? content[..500] + "..." : content;
            }
            catch
            {
                codePreview = "（无法读取文件内容）";
            }

            templates.Add(new ScriptTemplate
            {
                Name = name,
                FilePath = file,
                CodePreview = codePreview,
                Language = language,
                Description = $"脚本模板 ({language})",
                CompatibleEngineVersion = "4.x",
                CreatedAt = File.GetCreationTime(file)
            });
        }
        return templates;
    }
}
