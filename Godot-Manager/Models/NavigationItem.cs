namespace Godot_Manager.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// 左侧导航栏的导航项数据模型。
/// </summary>
public class NavigationItem : INotifyPropertyChanged
{
    /// <summary>导航项显示名称</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>导航项图标文本（可替换为 Icon 路径）</summary>
    public string Icon { get; init; } = string.Empty;

    /// <summary>关联的视图类型标识</summary>
    public string ViewKey { get; init; } = string.Empty;

    /// <summary>是否被选中</summary>
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// 属性变更事件。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 触发属性变更通知。
    /// </summary>
    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
