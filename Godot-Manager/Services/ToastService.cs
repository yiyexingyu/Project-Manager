namespace Godot_Manager.Services;

/// <summary>
/// Toast 通知类型枚举。
/// </summary>
public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

/// <summary>
/// Toast 通知消息模型。
/// </summary>
public class ToastMessage
{
    /// <summary>通知内容文本</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>通知类型</summary>
    public ToastType Type { get; init; } = ToastType.Info;

    /// <summary>显示时间戳</summary>
    public DateTime Timestamp { get; init; } = DateTime.Now;

    /// <summary>自动消失时长（毫秒），默认 3000ms</summary>
    public int DurationMs { get; init; } = 3000;
}

/// <summary>
/// 全局 Toast 通知服务。
/// 通过事件机制通知 UI 层显示轻量化提示。
/// </summary>
public class ToastService
{
    /// <summary>
    /// 当新 Toast 消息需要显示时触发。
    /// </summary>
    public event Action<ToastMessage>? ToastRequested;

    /// <summary>
    /// 显示成功通知。
    /// </summary>
    public void ShowSuccess(string message)
    {
        Show(new ToastMessage { Message = message, Type = ToastType.Success });
    }

    /// <summary>
    /// 显示错误通知。
    /// </summary>
    public void ShowError(string message)
    {
        Show(new ToastMessage { Message = message, Type = ToastType.Error, DurationMs = 5000 });
    }

    /// <summary>
    /// 显示警告通知。
    /// </summary>
    public void ShowWarning(string message)
    {
        Show(new ToastMessage { Message = message, Type = ToastType.Warning, DurationMs = 4000 });
    }

    /// <summary>
    /// 显示信息通知。
    /// </summary>
    public void ShowInfo(string message)
    {
        Show(new ToastMessage { Message = message, Type = ToastType.Info });
    }

    /// <summary>
    /// 触发 Toast 通知事件。
    /// </summary>
    private void Show(ToastMessage message)
    {
        ToastRequested?.Invoke(message);
    }
}
