using System.Windows;
using System.Windows.Input;

namespace Godot_Manager.Views;

public partial class TemplateView : System.Windows.Controls.UserControl
{
    public TemplateView()
    {
        InitializeComponent();
    }

    private void TemplateTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Primitives.ToggleButton btn) return;
        var tag = btn.Tag?.ToString();

        TabProject.IsChecked = tag == "项目模板";
        TabScript.IsChecked = tag == "脚本模板";

        ProjectTemplateList.Visibility = tag == "项目模板" ? Visibility.Visible : Visibility.Collapsed;
        ScriptTemplateList.Visibility = tag == "脚本模板" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void TemplateItem_Click(object sender, MouseButtonEventArgs e)
    {
        var dc = (sender as FrameworkElement)?.DataContext;
        var vm = DataContext as ViewModels.TemplateViewModel;
        if (vm == null || dc == null) return;

        if (dc is Models.ProjectTemplate pt)
        {
            vm.SelectedProjectTemplate = pt;
            vm.SelectedScriptTemplate = null;
            ProjectPreview.Visibility = Visibility.Visible;
            ScriptPreview.Visibility = Visibility.Collapsed;
            NoSelection.Visibility = Visibility.Collapsed;
        }
        else if (dc is Models.ScriptTemplate st)
        {
            vm.SelectedScriptTemplate = st;
            vm.SelectedProjectTemplate = null;
            ScriptPreview.Visibility = Visibility.Visible;
            ProjectPreview.Visibility = Visibility.Collapsed;
            NoSelection.Visibility = Visibility.Collapsed;
        }
    }
}
