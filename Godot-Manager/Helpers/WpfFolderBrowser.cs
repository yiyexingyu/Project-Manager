using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Godot_Manager.Helpers;

/// <summary>
/// WPF 原生文件夹选择对话框。
/// 通过 Windows Shell COM 接口 (IFileOpenDialog) 实现，非 WinForm。
/// 符合规则要求：仅使用 WPF 原生方案。
/// </summary>
public static class WpfFolderBrowser
{
    /// <summary>
    /// 显示文件夹选择对话框。
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="initialPath">初始路径（可选）</param>
    /// <returns>用户选择的文件夹路径，取消则返回 null</returns>
    public static string? ShowDialog(string title = "选择文件夹", string? initialPath = null)
    {
        try
        {
            var dialog = (NativeMethods.IFileOpenDialog)new NativeMethods.FileOpenDialog();
            dialog.SetOptions(
                NativeMethods.FOS.FOS_PICKFOLDERS |
                NativeMethods.FOS.FOS_FORCEFILESYSTEM |
                NativeMethods.FOS.FOS_PATHMUSTEXIST |
                NativeMethods.FOS.FOS_FILEMUSTEXIST);

            dialog.SetTitle(title);

            if (!string.IsNullOrEmpty(initialPath) && Directory.Exists(initialPath))
            {
                // 通过 IShellItem 设置初始文件夹
                var shellItem = NativeMethods.SHCreateItemFromParsingName(initialPath, IntPtr.Zero, typeof(NativeMethods.IShellItem).GUID);
                if (shellItem != null)
                {
                    dialog.SetFolder((NativeMethods.IShellItem)shellItem);
                }
            }

            var hwnd = Application.Current?.MainWindow;
            IntPtr ownerHandle = hwnd != null
                ? new WindowInteropHelper(hwnd).Handle
                : IntPtr.Zero;

            int hr = dialog.Show(ownerHandle);
            if (hr < 0) return null;

            dialog.GetResult(out var pItem);
            pItem.GetDisplayName(NativeMethods.SIGDN.SIGDN_FILESYSPATH, out var selectedPath);
            return selectedPath;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Windows Shell COM 接口定义（内部用）。
    /// </summary>
    private static class NativeMethods
    {
        [ComImport, Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        internal class FileOpenDialog { }

        [ComImport, Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileOpenDialog
        {
            [PreserveSig] int Show(IntPtr parent);
            void SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);
            void SetFileTypeIndex(uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise(IntPtr pfde, out uint pdwCookie);
            void Unadvise(uint dwCookie);
            void SetOptions(FOS fos);
            void GetOptions(out FOS pfos);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, int alignment);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close(int hr);
            void SetClientGuid(ref Guid guid);
            void ClearClientData();
            void SetFilter(IntPtr pFilter);
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItem
        {
            void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        internal enum SIGDN : uint
        {
            SIGDN_FILESYSPATH = 0x80058000
        }

        [Flags]
        internal enum FOS : uint
        {
            FOS_PICKFOLDERS = 0x00000020,
            FOS_FORCEFILESYSTEM = 0x00000040,
            FOS_PATHMUSTEXIST = 0x00000800,
            FOS_FILEMUSTEXIST = 0x00001000,
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern object SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            IntPtr pbc,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid);
    }
}
