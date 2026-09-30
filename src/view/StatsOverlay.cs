using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

using Color = System.Windows.Media.Color;
using Orientation = System.Windows.Controls.Orientation;

namespace TypeAestetic.View;

/// <summary>
/// Киберпанк OLED-панель статистики: WPM, статус, счетчик и стрик.
/// </summary>
public class StatsOverlay : Border
{
    private readonly TextBlock _wpmText;
    private readonly TextBlock _keystrokesText;
    private readonly TextBlock _streakText;
    private readonly Ellipse _statusDot;

    private readonly Queue<DateTime> _recentKeys = new();
    private readonly DispatcherTimer _updateTimer;

    private int _totalKeystrokes;
    private int _currentStreak;
    private DateTime _lastKeyTime = DateTime.MinValue;
    private const double StreakTimeoutSeconds = 2.0;
    private const double WpmWindowSeconds = 8.0;
    private const double CharsPerWord = 5.0;

    public StatsOverlay()
    {
        Height = 36;
        CornerRadius = new CornerRadius(8);
        Background = new SolidColorBrush(Color.FromArgb(230, 15, 17, 26));
        BorderBrush = new LinearGradientBrush(
            Color.FromArgb(90, 0, 229, 255),
            Color.FromArgb(20, 255, 255, 255),
            new Point(0, 0), new Point(1, 0));
        BorderThickness = new Thickness(1);
        Padding = new Thickness(14, 0, 14, 0);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // WPM блок
        var wpmPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        
        _statusDot = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = new SolidColorBrush(Color.FromRgb(0, 229, 255)),
            Margin = new Thickness(0, 0, 8, 0),
            Effect = new DropShadowEffect { Color = Color.FromRgb(0, 229, 255), BlurRadius = 8, ShadowDepth = 0 }
        };

        _wpmText = new TextBlock
        {
            Text = "0",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            FontFamily = new FontFamily("Consolas, Segoe UI"),
            Foreground = new SolidColorBrush(Color.FromRgb(0, 229, 255)),
            VerticalAlignment = VerticalAlignment.Center
        };

        var wpmLabel = new TextBlock
        {
            Text = " WPM",
            FontSize = 10,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(140, 0, 229, 255)),
            VerticalAlignment = VerticalAlignment.Center
        };

        wpmPanel.Children.Add(_statusDot);
        wpmPanel.Children.Add(_wpmText);
        wpmPanel.Children.Add(wpmLabel);
        Grid.SetColumn(wpmPanel, 0);
        grid.Children.Add(wpmPanel);

        // Центр: бейдж
        var brand = new TextBlock
        {
            Text = "TYPEAESTETIC // 75%",
            FontSize = 9,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(brand, 1);
        grid.Children.Add(brand);

        // Keystrokes
        _keystrokesText = new TextBlock
        {
            Text = "0 KEYS",
            FontSize = 10,
            FontWeight = FontWeights.SemiBold,
            FontFamily = new FontFamily("Consolas, Segoe UI"),
            Foreground = new SolidColorBrush(Color.FromArgb(170, 200, 215, 235)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 16, 0)
        };
        Grid.SetColumn(_keystrokesText, 2);
        grid.Children.Add(_keystrokesText);

        // Streak
        _streakText = new TextBlock
        {
            Text = "STREAK 0",
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 170, 30)),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_streakText, 3);
        grid.Children.Add(_streakText);

        Child = grid;

        _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _updateTimer.Tick += (_, _) => UpdateDisplay();
        _updateTimer.Start();
    }

    public void RecordKeystroke()
    {
        var now = DateTime.UtcNow;
        _totalKeystrokes++;
        _recentKeys.Enqueue(now);

        if ((now - _lastKeyTime).TotalSeconds <= StreakTimeoutSeconds)
            _currentStreak++;
        else
            _currentStreak = 1;

        _lastKeyTime = now;
    }

    private void UpdateDisplay()
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddSeconds(-WpmWindowSeconds);

        while (_recentKeys.Count > 0 && _recentKeys.Peek() < cutoff)
            _recentKeys.Dequeue();

        double wpm = (_recentKeys.Count / CharsPerWord) / (WpmWindowSeconds / 60.0);
        _wpmText.Text = ((int)Math.Round(wpm)).ToString();
        _keystrokesText.Text = $"{_totalKeystrokes:N0} KEYS";

        if ((now - _lastKeyTime).TotalSeconds > StreakTimeoutSeconds)
            _currentStreak = 0;

        _streakText.Text = _currentStreak >= 5 ? $"🔥 {_currentStreak}" : $"STREAK {_currentStreak}";
        _streakText.Foreground = _currentStreak >= 5
            ? new SolidColorBrush(Color.FromRgb(255, 170, 0))
            : new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
    }

    public void UpdateAccentColor(Color color)
    {
        _wpmText.Foreground = new SolidColorBrush(color);
        _statusDot.Fill = new SolidColorBrush(color);
    }
}