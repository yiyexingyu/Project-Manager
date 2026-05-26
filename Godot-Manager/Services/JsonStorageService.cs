using System.Text.Json;
using System.IO;

namespace Godot_Manager.Services;

/// <summary>
/// 泛型 JSON 本地文件持久化服务。
/// 所有用户数据均通过此服务以 JSON 格式存储于 Data/ 目录下。
/// </summary>
/// <typeparam name="T">可序列化的数据类型</typeparam>
public class JsonStorageService<T> where T : class, new()
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// 初始化 JSON 存储服务。
    /// </summary>
    /// <param name="fileName">JSON 文件名（不含路径），如 "projects.json"</param>
    public JsonStorageService(string fileName)
    {
        var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);

        _filePath = Path.Combine(dataDir, fileName);
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    /// <summary>
    /// 异步从 JSON 文件加载数据。文件不存在时返回新的默认实例。
    /// </summary>
    /// <returns>反序列化后的数据对象</returns>
    public async Task<T> LoadAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
                return new T();

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[JsonStorage] 加载 {_filePath} 失败: {ex.Message}");
            return new T();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 异步将数据保存到 JSON 文件。
    /// </summary>
    /// <param name="data">要持久化的数据对象</param>
    public async Task SaveAsync(T data)
    {
        await _semaphore.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[JsonStorage] 保存 {_filePath} 失败: {ex.Message}");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 检查数据文件是否存在。
    /// </summary>
    public bool FileExists() => File.Exists(_filePath);

    /// <summary>
    /// 获取数据文件的完整路径。
    /// </summary>
    public string FilePath => _filePath;
}
