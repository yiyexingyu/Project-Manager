using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Godot_Manager.Services;
using Godot_Manager.ViewModels;

namespace Godot_Manager.Views;

public partial class ProjectListView : UserControl
{
    public ProjectListView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ProjectListViewModel oldVm)
            oldVm.NewProjectRequested -= OpenNewProjectDialog;

        if (e.NewValue is ProjectListViewModel newVm)
            newVm.NewProjectRequested += OpenNewProjectDialog;
    }

    private void OpenNewProjectDialog()
    {
        var vm = DataContext as ProjectListViewModel;
        if (vm == null) return;

        var configService = new AppConfigService();
        var toastService = new ToastService();
        var projectService = new ProjectService(configService, toastService);
        var dialogVm = new NewProjectViewModel(projectService, configService, toastService);

        dialogVm.CloseRequested += () =>
        {
            var dialog = FindDialog(dialogVm);
            dialog?.Close();
        };

        dialogVm.ProjectCreated += async () =>
        {
            await vm.ScanProjectsAsync();
        };

        var dialogWin = new NewProjectDialog { DataContext = dialogVm, Owner = Window.GetWindow(this) };
        dialogWin.ShowDialog();
    }

    private static NewProjectDialog? FindDialog(NewProjectViewModel vm)
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window is NewProjectDialog dialog && dialog.DataContext == vm)
                return dialog;
        }
        return null;
    }

    private void ProjectCard_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
            border.Effect = (DropShadowEffect?)FindResource("CardShadowHover");
    }

    private void ProjectCard_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
            border.Effect = (DropShadowEffect?)FindResource("CardShadow");
    }
}
