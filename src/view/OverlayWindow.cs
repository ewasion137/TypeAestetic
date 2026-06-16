using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace TypeAestetic.View;

public class OverlayWindow : Window
{
    public OverlayWindow()
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;

        Width = 820;
        Height = 300; // Slightly reduced height

        // Use WorkArea to avoid Taskbar overlap
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 10;
        Top = workArea.Bottom - Height - 10;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // Make it click-through using Win32
        var hwnd = new WindowInteropHelper(this).Handle;
        uint exStyle = GetWindowLong(hwnd, -20);
        SetWindowLong(hwnd, -20, exStyle | 0x00000020 | 0x00080000);
    }

    [DllImport("user32.dll")]
    static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
}