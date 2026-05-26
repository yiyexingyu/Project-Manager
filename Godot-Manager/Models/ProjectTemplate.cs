namespace Godot_Manager.Models;

/// <summary>
/// 项目模板数据模型。
/// 包含完整的 Godot 项目结构模板信息。
/// </summary>
public class ProjectTemplate
{
    /// <summary>模板名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>模板描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>模板存储目录路径</summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>适配的引擎版本</summary>
    public string CompatibleEngineVersion { get; set; } = string.Empty;

    /// <summary>适用场景</summary>
    public string? UseCase { get; set; }

    /// <summary>是否为新建项目默认模板</summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 脚本模板数据模型。
/// 用于新建脚本文件的模板。
/// </summary>
public class ScriptTemplate
{
    /// <summary>模板名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>模板描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>模板文件路径</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>代码内容预览（前 500 字符）</summary>
    public string CodePreview { get; set; } = string.Empty;

    /// <summary>适配的引擎版本</summary>
    public string CompatibleEngineVersion { get; set; } = string.Empty;

    /// <summary>脚本语言类型（GDScript / C# 等）</summary>
    public string Language { get; set; } = "GDScript";

    /// <summary>是否为新建项目默认脚本模板</summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
