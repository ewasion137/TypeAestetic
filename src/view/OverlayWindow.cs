using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using TypeAestetic.Main;

namespace TypeAestetic.View;

public class OverlayWindow : Window
{
    private readonly AppSettings _settings;

    public OverlayWindow(AppSettings settings)
    {
        _settings = settings;

        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;

        Width = 745;
        Height = 340;

        PositionWindow();

        _settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AppSettings.OverlayCorner))
                Dispatcher.Invoke(PositionWindow);
            else if (e.PropertyName == nameof(AppSettings.OverlayOpacity))
                Dispatcher.Invoke(() => Opacity = _settings.OverlayOpacity);
            else if (e.PropertyName == nameof(AppSettings.OverlayVisible))
                Dispatcher.Invoke(() => ToggleVisibility(_settings.OverlayVisible));
        };
    }

    private void PositionWindow()
    {
        var workArea = SystemParameters.WorkArea;
        const double margin = 16;

        switch (_settings.OverlayCorner)
        {
            case OverlayCorner.BottomRight:
                Left = workArea.Right - Width - margin;
                Top = workArea.Bottom - Height - margin;
                break;
            case OverlayCorner.BottomLeft:
                Left = workArea.Left + margin;
                Top = workArea.Bottom - Height - margin;
                break;
            case OverlayCorner.TopRight:
                Left = workArea.Right - Width - margin;
                Top = workArea.Top + margin;
                break;
            case OverlayCorner.TopLeft:
                Left = workArea.Left + margin;
                Top = workArea.Top + margin;
                break;
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;
        uint exStyle = GetWindowLong(hwnd, -20);
        SetWindowLong(hwnd, -20, exStyle | 0x00000020 | 0x00080000);

        PlaySlideIn();
    }

    private void PlaySlideIn()
    {
        var workArea = SystemParameters.WorkArea;
        double targetTop = Top;

        if (_settings.OverlayCorner == OverlayCorner.BottomRight || _settings.OverlayCorner == OverlayCorner.BottomLeft)
            Top = workArea.Bottom + 40;
        else
            Top = workArea.Top - Height - 40;

        Opacity = 0;

        var slideAnim = new DoubleAnimation
        {
            To = targetTop,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        BeginAnimation(TopProperty, slideAnim);

        var fadeIn = new DoubleAnimation
        {
            To = _settings.OverlayOpacity,
            Duration = TimeSpan.FromMilliseconds(450),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        BeginAnimation(OpacityProperty, fadeIn);
    }

    public void ToggleVisibility(bool visible)
    {
        var targetOpacity = visible ? _settings.OverlayOpacity : 0;
        var fade = new DoubleAnimation
        {
            To = targetOpacity,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        BeginAnimation(OpacityProperty, fade);
    }

    [DllImport("user32.dll")]
    static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
}