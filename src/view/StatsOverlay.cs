using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

// Explicitly resolve aliases to avoid WinForms conflicts
using Color = System.Windows.Media.Color;
using Orientation = System.Windows.Controls.Orientation;

namespace TypeAestetic.View;

/// <summary>
/// Typing statistics overlay — shows WPM, total keystrokes, and current streak.
/// </summary>
public class StatsOverlay : Border
{
    private readonly TextBlock _wpmText;
    private readonly TextBlock _wpmLabel;
    private readonly TextBlock _keystrokesText;
    private readonly TextBlock _streakText;

    private readonly Queue<DateTime> _recentKeys = new();
    private readonly DispatcherTimer _updateTimer;

    private int _totalKeystrokes;
    private int _currentStreak;
    private DateTime _lastKeyTime = DateTime.MinValue;
    private const double StreakTimeoutSeconds = 2.0;
    private const double WpmWindowSeconds = 10.0;

    // Standard "word" = 5 characters for WPM calculation
    private const double CharsPerWord = 5.0;

    public StatsOverlay()
    {
        // Container styling — frosted glass strip
        Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
        BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(8);
        Padding = new Thickness(16, 8, 16, 8);
        Margin = new Thickness(0, 0, 0, 8);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

        // WPM Panel
        var wpmPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        
        _wpmText = new TextBlock
        {
            Text = "0",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 229, 255)),
            VerticalAlignment = VerticalAlignment.Center, // Fixed: Center instead of Baseline
            Margin = new Thickness(0, 0, 4, 0)
        };
        
        _wpmLabel = new TextBlock
        {
            Text = "WPM",
            FontSize = 9,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(140, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center, // Fixed: Center instead of Baseline
            Margin = new Thickness(0, 0, 0, 1)
        };
        
        wpmPanel.Children.Add(_wpmText);
        wpmPanel.Children.Add(_wpmLabel);
        Grid.SetColumn(wpmPanel, 0);
        grid.Children.Add(wpmPanel);

        // Keystrokes
        _keystrokesText = new TextBlock
        {
            Text = "0 keys",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right, // Fixed: Explicit Enum
            Margin = new Thickness(0, 0, 16, 0)
        };
        Grid.SetColumn(_keystrokesText, 2);
        grid.Children.Add(_keystrokesText);

        // Streak
        _streakText = new TextBlock
        {
            Text = "",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(160, 255, 200, 50)),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right // Fixed: Explicit Enum
        };
        Grid.SetColumn(_streakText, 3);
        grid.Children.Add(_streakText);

        Child = grid;

        // Update timer — refresh WPM display every 200ms
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _updateTimer.Tick += (_, _) => UpdateDisplay();
        _updateTimer.Start();
    }

    public void RecordKeystroke()
    {
        var now = DateTime.UtcNow;
        _totalKeystrokes++;
        _recentKeys.Enqueue(now);

        // Check streak
        if ((now - _lastKeyTime).TotalSeconds <= StreakTimeoutSeconds)
            _currentStreak++;
        else
            _currentStreak = 1;

        _lastKeyTime = now;

        // Trim old entries immediately
        var cutoff = now.AddSeconds(-WpmWindowSeconds);
        while (_recentKeys.Count > 0 && _recentKeys.Peek() < cutoff)
            _recentKeys.Dequeue();
    }

    private void UpdateDisplay()
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddSeconds(-WpmWindowSeconds);

        while (_recentKeys.Count > 0 && _recentKeys.Peek() < cutoff)
            _recentKeys.Dequeue();

        // Formula: (chars / 5) / (seconds / 60)
        int keysInWindow = _recentKeys.Count;
        double wpm = (keysInWindow / CharsPerWord) / (WpmWindowSeconds / 60.0);
        
        _wpmText.Text = ((int)Math.Round(wpm)).ToString();
        _keystrokesText.Text = $"{_totalKeystrokes:N0} keys";

        if ((now - _lastKeyTime).TotalSeconds > StreakTimeoutSeconds)
            _currentStreak = 0;

        _streakText.Text = _currentStreak >= 5 ? $"🔥 {_currentStreak}" : "";
    }

    public void UpdateAccentColor(Color color)
    {
        _wpmText.Foreground = new SolidColorBrush(color);
    }
}