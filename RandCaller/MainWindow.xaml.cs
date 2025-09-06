using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace RandCaller
{
    public sealed partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x00080000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_THICKFRAME = 0x00040000;

        public MainWindow()
        {
            InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(null); // 无标题栏
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            double scale = GetScaleForWindow(hwnd);
            int width = (int)(160 * scale);
            int height = (int)(100 * scale);
            SetWindowSizeAndTopmost(width, height);
            HideSystemButtons();
            this.Activated += (s, e) => SetWindowSizeAndTopmost(width, height); // 强制置顶
        }

        private double GetScaleForWindow(IntPtr hwnd)
        {
            uint dpi = GetDpiForWindow(hwnd);
            return dpi / 96.0;
        }

        private void SetWindowSizeAndTopmost(int width, int height)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            NativeMethods.SetWindowPos(hwnd, new IntPtr(-1), 0, 0, width, height, 0x0002 | 0x0004 | 0x0040); // SWP_NOMOVE | SWP_NOZORDER | SWP_SHOWWINDOW
        }

        private void HideSystemButtons()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            int style = GetWindowLong(hwnd, GWL_STYLE);
            style &= ~WS_MINIMIZEBOX;
            style &= ~WS_MAXIMIZEBOX;
            style &= ~WS_SYSMENU;
            style &= ~WS_THICKFRAME; // 禁止调整窗口大小
            SetWindowLong(hwnd, GWL_STYLE, style);
        }

        private void OpenSettingsManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取 SettingsManager.exe 路径
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "SettingsManager", "bin", "Debug", "net8.0-windows10.0.26100.0", "SettingsManager.exe");
                exePath = Path.GetFullPath(exePath);
                if (!File.Exists(exePath))
                {
                    exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "SettingsManager", "bin", "Release", "net8.0-windows10.0.26100.0", "SettingsManager.exe");
                    exePath = Path.GetFullPath(exePath);
                }
                if (File.Exists(exePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "无法打开高级设置",
                        Content = $"未找到 SettingsManager.exe。请先编译 SettingsManager 项目。",
                        CloseButtonText = "确定",
                        XamlRoot = this.Content.XamlRoot
                    };
                    _ = dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "无法打开高级设置",
                    Content = $"错误: {ex.Message}",
                    CloseButtonText = "确定",
                    XamlRoot = this.Content.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        }
    }
}
