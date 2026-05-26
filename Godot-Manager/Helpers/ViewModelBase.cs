using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Godot_Manager.Helpers;

/// <summary>
/// MVVM ViewModel 基类，实现 INotifyPropertyChanged 接口，
/// 提供属性变更通知的通用 SetProperty 方法。
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 设置属性值并在变更时自动触发 PropertyChanged 事件。
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="field">属性对应的私有字段引用</param>
    /// <param name="value">新值</param>
    /// <param name="propertyName">属性名称（自动获取）</param>
    /// <returns>值是否发生了变更</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// 手动触发属性变更通知。
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
