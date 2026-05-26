namespace Godot_Manager.Models;

/// <summary>
/// Godot 项目数据模型。
/// 对应通过 project.godot 文件识别到的本地 Godot 项目。
/// </summary>
public class GodotProject
{
    /// <summary>项目名称（从 project.godot 文件解析或目录名推断）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>项目根目录完整路径</summary>
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>project.godot 文件的完整路径</summary>
    public string GodotFilePath { get; set; } = string.Empty;

    /// <summary>Godot 引擎主版本号（如 "4.3"）</summary>
    public string EngineVersion { get; set; } = string.Empty;

    /// <summary>关联的引擎可执行文件路径</summary>
    public string? AssociatedEnginePath { get; set; }

    /// <summary>项目最后修改时间</summary>
    public DateTime LastModified { get; set; }

    /// <summary>项目的简短描述（可选）</summary>
    public string? Description { get; set; }

    /// <summary>在列表中被发现/添加的时间</summary>
    public DateTime AddedAt { get; set; } = DateTime.Now;
}
