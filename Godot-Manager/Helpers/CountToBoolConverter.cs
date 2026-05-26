using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Godot_Manager.Helpers;

/// <summary>
/// 将数字值转换为 Visibility：0 → Visible（显示空状态），非0 → Collapsed。
/// </summary>
public class CountToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
