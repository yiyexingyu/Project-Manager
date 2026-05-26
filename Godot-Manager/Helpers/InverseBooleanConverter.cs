using System.Globalization;
using System.Windows.Data;

namespace Godot_Manager.Helpers;

/// <summary>
/// 布尔取反转换器。true → false, false → true。
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}
