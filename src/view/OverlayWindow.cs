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
        SizeToContent = SizeToContent.WidthAndHeight;

        // Position at bottom-right
        Left = SystemParameters.PrimaryScreenWidth - 450; 
        Top = SystemParameters.PrimaryScreenHeight - 250;
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