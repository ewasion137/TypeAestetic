using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace TypeAestetic.View;

public class OverlayWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    private readonly Canvas _canvas;

    public OverlayWindow()
    {
        // Config
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        
        // Fullscreen
        Left = 0;
        Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;

        _canvas = new Canvas();
        Content = _canvas;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        // WS_EX_TRANSPARENT
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    public void SpawnLetter(char character)
    {
        // Test
        var random = new Random();
        var textBlock = new TextBlock
        {
            Text = character.ToString(),
            FontSize = 48,
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold
        };

        Canvas.SetLeft(textBlock, random.Next(100, (int)Width - 100));
        Canvas.SetTop(textBlock, random.Next(100, (int)Height - 100));
        _canvas.Children.Add(textBlock);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}