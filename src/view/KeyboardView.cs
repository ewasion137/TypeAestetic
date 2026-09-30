using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using TypeAestetic.Main;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace TypeAestetic.View;

using FontFamily = System.Windows.Media.FontFamily;


public class KeyboardView : Canvas
{
    private readonly HashSet<string> _pressedKeys = new();
    private readonly Dictionary<string, Border> _keys = new();
    private readonly Dictionary<string, Point> _keyPositions = new();

    private ParticleEffect? _particles;
    private StatsOverlay? _stats;
    private Color _accentColor = Color.FromRgb(0, 229, 255); // Electric cyan

    // Breathing animation
    private Storyboard? _breatheStoryboard;

    public StatsOverlay? Stats => _stats;

    public KeyboardView()
    {
        Width = 730;
        Height = 320;
        ClipToBounds = false;

        BuildUI();
        StartBreathingAnimation();
    }

    public void SetAccentColor(Color color)
    {
        _accentColor = color;
        if (_particles != null) _particles.AccentColor = color;
        _stats?.UpdateAccentColor(color);
    }

    private void BuildUI()
    {
        // Внешнее неоновое свечение шасси
        var ambientGlow = new Border
        {
            Width = 718,
            Height = 300,
            CornerRadius = new CornerRadius(14),
            Background = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.5),
                RadiusX = 0.7,
                RadiusY = 0.7,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(30, 0, 229, 255), 0),
                    new GradientStop(Color.FromArgb(8, 0, 229, 255), 0.6),
                    new GradientStop(Color.FromArgb(0, 0, 0, 0), 1)
                }
            },
            Opacity = 0.9
        };
        Canvas.SetLeft(ambientGlow, 6);
        Canvas.SetTop(ambientGlow, 6);
        Children.Add(ambientGlow);

        // Монолитное обсидиановое шасси (непрозрачное!)
        var chassis = new Border
        {
            Width = 710,
            Height = 295,
            CornerRadius = new CornerRadius(12),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(18, 20, 30), 0),
                    new GradientStop(Color.FromRgb(12, 13, 20), 1)
                }
            },
            BorderBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(120, 0, 229, 255), 0),
                    new GradientStop(Color.FromArgb(40, 255, 255, 255), 0.5),
                    new GradientStop(Color.FromArgb(20, 0, 229, 255), 1)
                }
            },
            BorderThickness = new Thickness(1.2),
            Effect = new DropShadowEffect
            {
                BlurRadius = 25,
                ShadowDepth = 6,
                Opacity = 0.8,
                Color = Colors.Black
            }
        };
        Canvas.SetLeft(chassis, 10);
        Canvas.SetTop(chassis, 10);
        Children.Add(chassis);

        // OLED дашборд
        _stats = new StatsOverlay { Width = 686 };
        Canvas.SetLeft(_stats, 22);
        Canvas.SetTop(_stats, 18);
        Children.Add(_stats);

        // Клавиатурный блок
        CreateLayout();

        // Слой эффектов
        var particleCanvas = new Canvas
        {
            Width = 730,
            Height = 320,
            ClipToBounds = false
        };
        Children.Add(particleCanvas);
        _particles = new ParticleEffect(particleCanvas) { AccentColor = _accentColor };
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

        double baseX = 22;
        double baseY = 62;
        double gap = 4;
        double rowGap = 4;

        double currentY = baseY;

        for (int r = 0; r < rows.Length; r++)
        {
            var row = rows[r];
            double currentX = baseX;
            double keyHeight = (r == 0) ? 28 : 34;

            if (r == 0)
            {
                // Идеальное 75% распределение кластеров F-ряда
                for (int i = 0; i < row.Length; i++)
                {
                    var key = row[i];
                    double w = (key == "Escape") ? 44 : 42;
                    AddKey(key, currentX, currentY, w, keyHeight);
                    currentX += w;

                    if (key == "Escape" || key == "F4" || key == "F8")
                        currentX += 34; // кластерные зазоры
                    else if (i < row.Length - 1)
                        currentX += gap;
                }
            }
            else
            {
                foreach (var key in row)
                {
                    double width = GetKeyWidth(key);
                    AddKey(key, currentX, currentY, width, keyHeight);
                    currentX += width + gap;
                }
            }

            currentY += keyHeight + rowGap;
        }
    }

    private double GetKeyWidth(string key)
    {
        return key switch
        {
            "Back" => 88,
            "Tab" => 65,
            "Oem5" => 65,
            "Capital" => 76,
            "Return" => 100,
            "LeftShift" => 99,
            "RightShift" => 123,
            "LeftCtrl" or "LWin" or "LeftAlt" or "RightAlt" or "RWin" or "Apps" or "RightCtrl" => 53,
            "Space" => 287,
            _ => 42
        };
    }

    private void AddKey(string keyName, double x, double y, double width, double height)
    {
        string displayLabel = keyName switch
        {
            "Oem3" => "~",
            "OemMinus" => "-",
            "OemPlus" => "+",
            "Back" => "⌫",
            "OemOpenBrackets" => "[",
            "Oem6" => "]",
            "Oem5" => "\\",
            "Oem1" => ";",
            "OemQuotes" => "'",
            "Return" => "↵",
            "OemComma" => ",",
            "OemPeriod" => ".",
            "OemQuestion" => "/",
            "Capital" => "CAPS",
            "LeftShift" or "RightShift" => "⇧",
            "LeftCtrl" or "RightCtrl" => "CTRL",
            "LeftAlt" or "RightAlt" => "ALT",
            "Space" => "",
            "LWin" or "RWin" => "⊞",
            "Escape" => "ESC",
            "Apps" => "☰",
            "Tab" => "⇥",
            _ => (keyName.Length > 1 && keyName.StartsWith("D") && char.IsDigit(keyName[1]))
                 ? keyName.Substring(1)
                 : keyName
        };

        bool isMod = keyName is "LeftCtrl" or "RightCtrl" or "LeftAlt" or "RightAlt" or "LWin" or "RWin"
                     or "Apps" or "Capital" or "LeftShift" or "RightShift" or "Return" or "Tab" or "Back" or "Escape";

        // Тактильный кейкап с 2.5D градиентом
        var keyBg = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(isMod ? Color.FromRgb(25, 27, 38) : Color.FromRgb(32, 35, 48), 0),
                new GradientStop(isMod ? Color.FromRgb(16, 17, 24) : Color.FromRgb(21, 23, 32), 1)
            }
        };

        var borderBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(70, 255, 255, 255), 0), // свет сверху
                new GradientStop(Color.FromArgb(20, 255, 255, 255), 0.5),
                new GradientStop(Color.FromArgb(80, 0, 0, 0), 1)        // фаска снизу
            }
        };

        UIElement keyContent;
        if (keyName == "Space")
        {
            keyContent = new Border
            {
                Width = 56,
                Height = 3,
                CornerRadius = new CornerRadius(1.5),
                Background = new SolidColorBrush(Color.FromArgb(90, 0, 229, 255)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }
        else
        {
            keyContent = new TextBlock
            {
                Text = displayLabel,
                Foreground = isMod
                    ? new SolidColorBrush(Color.FromArgb(170, 160, 175, 195))
                    : new SolidColorBrush(Color.FromRgb(235, 240, 250)),
                FontSize = displayLabel.Length > 3 ? 8 : 10,
                FontWeight = FontWeights.SemiBold,
                FontFamily = new FontFamily("Segoe UI, Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        var tg = new TransformGroup();
        tg.Children.Add(new ScaleTransform(1.0, 1.0));
        tg.Children.Add(new TranslateTransform(0, 0));

        var border = new Border
        {
            Width = width,
            Height = height,
            Background = keyBg,
            CornerRadius = new CornerRadius(5),
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1, 1, 1, 1.5),
            Child = keyContent,
            Effect = new DropShadowEffect
            {
                BlurRadius = 3,
                ShadowDepth = 1.5,
                Direction = 270,
                Opacity = 0.5,
                Color = Colors.Black
            },
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = tg
        };

        Canvas.SetLeft(border, x);
        Canvas.SetTop(border, y);
        Children.Add(border);

        var upperKey = keyName.ToUpper();
        _keys[upperKey] = border;
        _keyPositions[upperKey] = new Point(x + width / 2, y + height / 2);
    }

    public void PressKey(string key)
    {
        key = key.ToUpper();
        if (_pressedKeys.Contains(key)) return;
        _pressedKeys.Add(key);

        _stats?.RecordKeystroke();

        if (_keys.TryGetValue(key, out var border))
        {
            border.BeginAnimation(Border.OpacityProperty, null);

            // Неоновая подсветка
            border.Background = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.4),
                RadiusX = 0.9,
                RadiusY = 0.9,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(230, _accentColor.R, _accentColor.G, _accentColor.B), 0),
                    new GradientStop(Color.FromArgb(130, _accentColor.R, _accentColor.G, _accentColor.B), 0.6),
                    new GradientStop(Color.FromArgb(40, _accentColor.R, _accentColor.G, _accentColor.B), 1)
                }
            };

            border.BorderBrush = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255));
            border.Effect = new DropShadowEffect
            {
                Color = _accentColor,
                BlurRadius = 18,
                ShadowDepth = 0,
                Opacity = 0.95
            };

            // Тактильный щелчок вниз
            if (border.RenderTransform is TransformGroup tg &&
                tg.Children[0] is ScaleTransform scale &&
                tg.Children[1] is TranslateTransform trans)
            {
                var down = new DoubleAnimation(0.96, TimeSpan.FromMilliseconds(50))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, down);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, down);

                var move = new DoubleAnimation(1.5, TimeSpan.FromMilliseconds(50));
                trans.BeginAnimation(TranslateTransform.YProperty, move);
            }

            if (border.Child is TextBlock tb)
                tb.Foreground = Brushes.White;

            if (_keyPositions.TryGetValue(key, out var pos))
                _particles?.Emit(pos.X, pos.Y, 5);
        }
    }

    public void ReleaseKey(string key)
    {
        key = key.ToUpper();
        _pressedKeys.Remove(key);

        if (_keys.TryGetValue(key, out var border))
        {
            bool isMod = key is "LEFTCTRL" or "RIGHTCTRL" or "LEFTALT" or "RIGHTALT" or "LWIN" or "RWIN"
                         or "APPS" or "CAPITAL" or "LEFTSHIFT" or "RIGHTSHIFT" or "RETURN" or "TAB" or "BACK" or "ESCAPE";

            var defaultBg = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(isMod ? Color.FromRgb(25, 27, 38) : Color.FromRgb(32, 35, 48), 0),
                    new GradientStop(isMod ? Color.FromRgb(16, 17, 24) : Color.FromRgb(21, 23, 32), 1)
                }
            };
            border.Background = defaultBg;

            border.BorderBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(70, 255, 255, 255), 0),
                    new GradientStop(Color.FromArgb(20, 255, 255, 255), 0.5),
                    new GradientStop(Color.FromArgb(80, 0, 0, 0), 1)
                }
            };

            border.Effect = new DropShadowEffect
            {
                BlurRadius = 3,
                ShadowDepth = 1.5,
                Direction = 270,
                Opacity = 0.5,
                Color = Colors.Black
            };

            if (border.RenderTransform is TransformGroup tg &&
                tg.Children[0] is ScaleTransform scale &&
                tg.Children[1] is TranslateTransform trans)
            {
                var up = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 1, Springiness = 7 }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, up);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, up);

                var moveBack = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(120));
                trans.BeginAnimation(TranslateTransform.YProperty, moveBack);
            }

            if (border.Child is TextBlock tb)
            {
                tb.Foreground = isMod
                    ? new SolidColorBrush(Color.FromArgb(170, 160, 175, 195))
                    : new SolidColorBrush(Color.FromRgb(235, 240, 250));
            }
        }
    }

    public void SetParticlesEnabled(bool enabled)
    {
        if (_particles != null) _particles.Enabled = enabled;
    }

    private void StartBreathingAnimation()
    {
        // Subtle opacity pulse when idle — gives the overlay a "living" feel
        var breathe = new DoubleAnimation
        {
            From = 0.92,
            To = 0.98,
            Duration = TimeSpan.FromSeconds(3),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        this.BeginAnimation(OpacityProperty, breathe);
    }

}