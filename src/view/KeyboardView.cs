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
        string[][] rows = {
        new[] { "Escape", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" },
        new[] { "Oem3", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D0", "OemMinus", "OemPlus", "Back" },
        new[] { "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "OemOpenBrackets", "Oem6", "Oem5" },
        new[] { "Capital", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Oem1", "OemQuotes", "Return" },
        new[] { "LeftShift", "Z", "X", "C", "V", "B", "N", "M", "OemComma", "OemPeriod", "OemQuestion", "RightShift" },
        new[] { "LeftCtrl", "LWin", "LeftAlt", "Space", "RightAlt", "RWin", "Apps", "RightCtrl" }
    };

        double startY = 0;
        foreach (var row in rows)
        {
            double startX = 0;
            // Small offset for F-keys row to align with numbers
            if (row[0] == "Escape") startX = 0;

            foreach (var key in row)
            {
                double width = GetKeyWidth(key);
                AddKey(key, startX, startY, width);
                startX += width + 4; // Gap between keys
            }
            startY += 44; // Vertical gap
        }
    }

    // [WHERE: GetKeyWidth method - compact version]
    private double GetKeyWidth(string key)
    {
        return key switch
        {
            "Space" => 220,
            "LeftShift" => 90,
            "RightShift" => 105,
            "Return" => 80,
            "Back" => 75,
            "Tab" => 55,
            "Capital" => 70,
            "LeftCtrl" or "RightCtrl" => 60,
            "LeftAlt" or "RightAlt" => 50,
            "Escape" => 50,
            _ => 40 // Standard key width
        };
    }

    // [WHERE: AddKey method - styling]
    private void AddKey(string keyName, double x, double y, double width)
    {
        // Simplified labels
        string displayLabel = keyName switch
        {
            "Oem3" => "~",
            "OemMinus" => "-",
            "OemPlus" => "=",
            "Back" => "BKSP",
            "OemOpenBrackets" => "[",
            "Oem6" => "]",
            "Oem5" => "\\",
            "Oem1" => ";",
            "OemQuotes" => "'",
            "Return" => "ENT",
            "OemComma" => ",",
            "OemPeriod" => ".",
            "OemQuestion" => "/",
            "Capital" => "CAPS",
            "LeftShift" => "SHIFT",
            "RightShift" => "SHIFT",
            "LeftCtrl" => "CTRL",
            "RightCtrl" => "CTRL",
            "LeftAlt" => "ALT",
            "RightAlt" => "ALT",
            "Space" => "",
            "LWin" or "RWin" => "WIN",
            // Фикс для D: Убираем D только если это цифра (D1, D2...), иначе оставляем
            _ => (keyName.Length > 1 && keyName.StartsWith("D") && char.IsDigit(keyName[1]))
                 ? keyName.Substring(1)
                 : keyName.Replace("Oem", "")
        };

        var border = new Border
        {
            Width = width,
            Height = 40, // Standard height
            Background = new SolidColorBrush(Color.FromArgb(40, 10, 10, 10)), // Darker glass
            CornerRadius = new CornerRadius(3),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            Child = new TextBlock
            {
                Text = displayLabel,
                Foreground = Brushes.White,
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };

        Canvas.SetLeft(border, x);
        Canvas.SetTop(border, y);
        Children.Add(border);
        _keys[keyName.ToUpper()] = border;
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

            // Fix: Use Capital 'C' in Color
            border.Effect = new DropShadowEffect
            {
                Color = Colors.Cyan,
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.8
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