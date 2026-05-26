using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Godot_Manager.Views;

public partial class PluginView : UserControl
{
    public PluginView()
    {
        InitializeComponent();
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton btn) return;
        var tag = btn.Tag?.ToString();

        // 取消其他 Tab 选中
        TabLocal.IsChecked = tag == "本地插件";
        TabOfficial.IsChecked = tag == "官方插件";
        TabFavorites.IsChecked = tag == "收藏插件";

        // 切换视图可见性
        LocalTabView.Visibility = tag == "本地插件" ? Visibility.Visible : Visibility.Collapsed;
        OfficialTabView.Visibility = tag == "官方插件" ? Visibility.Visible : Visibility.Collapsed;
        FavoritesTabView.Visibility = tag == "收藏插件" ? Visibility.Visible : Visibility.Collapsed;
    }
}
