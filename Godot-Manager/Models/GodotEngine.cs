namespace Godot_Manager.Models;

/// <summary>
/// Godot 引擎数据模型。
/// </summary>
public class GodotEngine
{
    /// <summary>版本号（如 "4.3"）</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>引擎可执行文件完整路径</summary>
    public string ExecutablePath { get; set; } = string.Empty;

    /// <summary>引擎安装目录</summary>
    public string InstallPath { get; set; } = string.Empty;

    /// <summary>主版本分支：3.x 或 4.x</summary>
    public string Branch { get; set; } = string.Empty;

    /// <summary>发布类型：正式版 / 测试版</summary>
    public string ReleaseType { get; set; } = "正式版";

    /// <summary>是否为默认启动引擎</summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>发现时间</summary>
    public DateTime DiscoveredAt { get; set; } = DateTime.Now;
}
