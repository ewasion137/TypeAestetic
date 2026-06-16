using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace TypeAestetic.View;

public class KeyboardView : Canvas
{
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
        if (!_keys.TryGetValue(key.ToUpper(), out var border)) return;

        // Visual Feedback: Glow and Scale
        var anim = new ColorAnimation(Colors.Cyan, TimeSpan.FromMilliseconds(50)) { AutoReverse = true };
        border.Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);
        
        // Sound could be triggered here too
    }
}