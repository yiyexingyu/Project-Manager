using System.Collections.ObjectModel;
using System.IO;
using Godot_Manager.Helpers;
using Godot_Manager.Models;
using Godot_Manager.Services;

namespace Godot_Manager.ViewModels;

/// <summary>
/// 模板管理 ViewModel。
/// 管理项目模板和脚本模板的全生命周期。
/// </summary>
public class TemplateViewModel : ViewModelBase
{
    private readonly TemplateService _templateService;
    private readonly AppConfigService _configService;
    private readonly ToastService _toastService;
    private string _activeTemplateType = "项目模板";
    private bool _isLoading;
    private ProjectTemplate? _selectedProjectTemplate;
    private ScriptTemplate? _selectedScriptTemplate;

    public TemplateViewModel()
    {
        _templateService = new TemplateService();
        _configService = new AppConfigService();
        _toastService = new ToastService();

        RefreshCommand = new AsyncRelayCommand(LoadTemplatesAsync);
        SetDefaultCommand = new RelayCommand<string>(SetDefaultTemplate);
        NewTemplateCommand = new RelayCommand(() => _toastService.ShowInfo("新建模板功能即将完善"));
        ImportTemplateCommand = new RelayCommand(() => _toastService.ShowInfo("导入模板功能即将完善"));
        DeleteTemplateCommand = new RelayCommand<string>(DeleteTemplate);

        _ = LoadTemplatesAsync();
    }

    public string PageTitle => "模板管理";
    public string ActiveTemplateType { get => _activeTemplateType; set { if (SetProperty(ref _activeTemplateType, value)) OnPropertyChanged(nameof(IsProjectTemplate)); } }
    public bool IsProjectTemplate => ActiveTemplateType == "项目模板";
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public ProjectTemplate? SelectedProjectTemplate { get => _selectedProjectTemplate; set => SetProperty(ref _selectedProjectTemplate, value); }
    public ScriptTemplate? SelectedScriptTemplate { get => _selectedScriptTemplate; set => SetProperty(ref _selectedScriptTemplate, value); }

    public ObservableCollection<ProjectTemplate> ProjectTemplates { get; } = [];
    public ObservableCollection<ScriptTemplate> ScriptTemplates { get; } = [];

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand<string> SetDefaultCommand { get; }
    public RelayCommand NewTemplateCommand { get; }
    public RelayCommand ImportTemplateCommand { get; }
    public RelayCommand<string> DeleteTemplateCommand { get; }

    private async Task LoadTemplatesAsync()
    {
        IsLoading = true;
        try
        {
            var config = await _configService.GetConfigAsync();
            var (projectTmpls, scriptTmpls) = await _templateService.ScanTemplatesAsync();

            ProjectTemplates.Clear();
            foreach (var t in projectTmpls)
            {
                t.IsDefault = config.DefaultProjectTemplateName == t.Name;
                ProjectTemplates.Add(t);
            }

            ScriptTemplates.Clear();
            foreach (var t in scriptTmpls)
            {
                t.IsDefault = config.DefaultScriptTemplateName == t.Name;
                ScriptTemplates.Add(t);
            }
        }
        catch (Exception ex) { _toastService.ShowError($"加载模板失败: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async void SetDefaultTemplate(string? name)
    {
        if (string.IsNullOrEmpty(name)) return;
        var config = await _configService.GetConfigAsync();

        if (IsProjectTemplate)
        {
            config.DefaultProjectTemplateName = config.DefaultProjectTemplateName == name ? null : name;
            foreach (var t in ProjectTemplates) t.IsDefault = t.Name == config.DefaultProjectTemplateName;
        }
        else
        {
            config.DefaultScriptTemplateName = config.DefaultScriptTemplateName == name ? null : name;
            foreach (var t in ScriptTemplates) t.IsDefault = t.Name == config.DefaultScriptTemplateName;
        }
        await _configService.SaveConfigAsync();
    }

    private async void DeleteTemplate(string? name)
    {
        if (string.IsNullOrEmpty(name)) return;
        try
        {
            if (IsProjectTemplate)
            {
                var item = ProjectTemplates.FirstOrDefault(t => t.Name == name);
                if (item != null) ProjectTemplates.Remove(item);
            }
            else
            {
                var item = ScriptTemplates.FirstOrDefault(t => t.Name == name);
                if (item != null) ScriptTemplates.Remove(item);
            }
            _toastService.ShowSuccess($"模板 '{name}' 已删除");
        }
        catch (Exception ex) { _toastService.ShowError($"删除失败: {ex.Message}"); }
        await Task.CompletedTask;
    }
}

