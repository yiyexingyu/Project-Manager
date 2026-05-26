using System.Windows;
using System.Windows.Input;

namespace Godot_Manager.Views;

/// <summary>
/// 新建项目弹窗交互逻辑。
/// </summary>
public partial class NewProjectDialog : Window
{
    public NewProjectDialog()
    {
        InitializeComponent();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) return;
        DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
