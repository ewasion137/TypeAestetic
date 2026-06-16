using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Media.Effects;

namespace TypeAestetic.View;

public class KeyboardView : Canvas
{
    private readonly HashSet<string> _pressedKeys = new();
    private readonly Dictionary<string, Border> _keys = new();

    public KeyboardView()
    {
        Width = 400;
        Height = 150;
        CreateLayout();
    }

    private void CreateLayout()
    {
        // Simple WASD + Space layout for demo
        AddKey("W", 50, 0);
        AddKey("A", 0, 50);
        AddKey("S", 50, 50);
        AddKey("D", 100, 50);
        AddKey("Space", 0, 100, 150);
    }

    private void AddKey(string label, double x, double y, double width = 45)
    {
        var border = new Border
        {
            Width = width,
            Height = 45,
            Background = new SolidColorBrush(Color.FromArgb(100, 30, 30, 30)),
            CornerRadius = new CornerRadius(5),
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Child = new TextBlock 
            { 
                Text = label, 
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };

        SetLeft(border, x);
        SetTop(border, y);
        Children.Add(border);
        _keys[label.ToUpper()] = border;
    }

    public void PressKey(string key)
    {
        key = key.ToUpper();
        if (_pressedKeys.Contains(key)) return; // Filter Windows repeat-key lag
        _pressedKeys.Add(key);

        if (_keys.TryGetValue(key, out var border))
        {
            // Cancel previous animations and set "Active" state
            border.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
            border.Background = new SolidColorBrush(Colors.Cyan);
        
            // Add Glow effect
            border.Effect = new DropShadowEffect { 
                color = Colors.Cyan, BlurRadius = 15, ShadowDepth = 0, Opacity = 0.8 
            };
        }
    }

    public void ReleaseKey(string key)
    {
        key = key.ToUpper();
        _pressedKeys.Remove(key);

        if (_keys.TryGetValue(key, out var border))
        {
            // Smooth return to default state
            var fadeAnim = new ColorAnimation
            {
                To = Color.FromArgb(100, 30, 30, 30),
                Duration = TimeSpan.FromMilliseconds(200)
            };
            border.Background.BeginAnimation(SolidColorBrush.ColorProperty, fadeAnim);
        
            // Remove glow
            border.Effect = null;
        }
    }
}