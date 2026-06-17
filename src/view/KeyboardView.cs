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
        Width = 700;
        Height = 330;
        ClipToBounds = false;

        // Create the layered structure
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
        // === Background glow (ambient light behind the keyboard) ===
        var ambientGlow = new Border
        {
            Width = 680,
            Height = 300,
            CornerRadius = new CornerRadius(16),
            Background = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5),
                RadiusX = 0.7,
                RadiusY = 0.8,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(15, 0, 229, 255), 0),
                    new GradientStop(Color.FromArgb(5, 0, 229, 255), 0.6),
                    new GradientStop(Color.FromArgb(0, 0, 0, 0), 1)
                }
            },
            Opacity = 0.8
        };
        Canvas.SetLeft(ambientGlow, 10);
        Canvas.SetTop(ambientGlow, 22);
        Children.Add(ambientGlow);

        // === Frosted glass container ===
        var container = new Border
        {
            Width = 670,
            Height = 295,
            CornerRadius = new CornerRadius(14),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(35, 255, 255, 255), 0),
                    new GradientStop(Color.FromArgb(15, 255, 255, 255), 0.5),
                    new GradientStop(Color.FromArgb(25, 255, 255, 255), 1)
                }
            },
            BorderBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(80, 255, 255, 255), 0),
                    new GradientStop(Color.FromArgb(20, 255, 255, 255), 0.5),
                    new GradientStop(Color.FromArgb(60, 255, 255, 255), 1)
                }
            },
            BorderThickness = new Thickness(1),
            Effect = new BlurEffect { Radius = 0.5 } // Subtle glass blur
        };
        Canvas.SetLeft(container, 15);
        Canvas.SetTop(container, 25);
        Children.Add(container);

        // === Stats overlay (positioned above the keyboard) ===
        _stats = new StatsOverlay
        {
            Width = 660
        };
        Canvas.SetLeft(_stats, 20);
        Canvas.SetTop(_stats, 0);
        Children.Add(_stats);

        // === Keyboard keys ===
        CreateLayout();

        // === Particle canvas (on top of everything) ===
        var particleCanvas = new Canvas
        {
            Width = 700,
            Height = 330,
            ClipToBounds = false
        };
        Children.Add(particleCanvas);
        _particles = new ParticleEffect(particleCanvas);
        _particles.AccentColor = _accentColor;
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
        double baseY = 44; // After stats bar
        double keyHeight = 34;
        double gap = 3;
        double rowGap = 3;

        double startY = baseY;
        foreach (var row in rows)
        {
            double startX = baseX;

            foreach (var key in row)
            {
                double width = GetKeyWidth(key);
                AddKey(key, startX, startY, width, keyHeight);
                startX += width + gap;
            }
            startY += keyHeight + rowGap;
        }
    }

    private double GetKeyWidth(string key)
    {
        return key switch
        {
            "Space" => 178,
            "LeftShift" => 72,
            "RightShift" => 86,
            "Return" => 66,
            "Back" => 62,
            "Tab" => 46,
            "Capital" => 58,
            "LeftCtrl" or "RightCtrl" => 50,
            "LeftAlt" or "RightAlt" => 42,
            "Escape" => 42,
            "LWin" or "RWin" => 42,
            "Apps" => 42,
            _ => 42 // Standard key width
        };
    }

    private void AddKey(string keyName, double x, double y, double width, double height)
    {
        string displayLabel = keyName switch
        {
            "Oem3" => "~",
            "OemMinus" => "-",
            "OemPlus" => "=",
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

        // Key background — layered glass effect
        var keyBg = new LinearGradientBrush
        {
            StartPoint = new Point(0.5, 0),
            EndPoint = new Point(0.5, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(45, 255, 255, 255), 0),
                new GradientStop(Color.FromArgb(18, 255, 255, 255), 0.4),
                new GradientStop(Color.FromArgb(12, 255, 255, 255), 1)
            }
        };

        var border = new Border
        {
            Width = width,
            Height = height,
            Background = keyBg,
            CornerRadius = new CornerRadius(5),
            BorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
            BorderThickness = new Thickness(0.8),
            Child = new TextBlock
            {
                Text = displayLabel,
                Foreground = new SolidColorBrush(Color.FromArgb(210, 255, 255, 255)),
                FontSize = displayLabel.Length > 3 ? 7.5 : 9.5,
                FontWeight = FontWeights.Medium,
                FontFamily = new FontFamily("Segoe UI"),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            },

            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new ScaleTransform(1.0, 1.0)
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

        // Record stat
        _stats?.RecordKeystroke();

        if (_keys.TryGetValue(key, out var border))
        {
            // Cancel any running release animation
            border.BeginAnimation(Border.OpacityProperty, null);
            if (border.Background is SolidColorBrush)
                border.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);

            // === Accent gradient glow ===
            var accentBrush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5),
                RadiusX = 0.8,
                RadiusY = 0.8,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(200, _accentColor.R, _accentColor.G, _accentColor.B), 0),
                    new GradientStop(Color.FromArgb(120, _accentColor.R, _accentColor.G, _accentColor.B), 0.6),
                    new GradientStop(Color.FromArgb(40, _accentColor.R, _accentColor.G, _accentColor.B), 1)
                }
            };
            border.Background = accentBrush;

            // === Glow effect ===
            border.Effect = new DropShadowEffect
            {
                Color = _accentColor,
                BlurRadius = 20,
                ShadowDepth = 0,
                Opacity = 0.9
            };

            // === Border highlight ===
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(180, _accentColor.R, _accentColor.G, _accentColor.B));

            // === Spring-like scale animation (down → bounce → settle) ===
            var scale = border.RenderTransform as ScaleTransform ?? new ScaleTransform(1.0, 1.0);
            border.RenderTransform = scale;

            var pressDown = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(200)
            };
            pressDown.KeyFrames.Add(new EasingDoubleKeyFrame(0.92, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(40)),
                new CubicEase { EasingMode = EasingMode.EaseOut }));
            pressDown.KeyFrames.Add(new EasingDoubleKeyFrame(1.03, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120)),
                new CubicEase { EasingMode = EasingMode.EaseOut }));
            pressDown.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)),
                new CubicEase { EasingMode = EasingMode.EaseInOut }));

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, pressDown);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, pressDown.Clone());

            // === Text brighten ===
            if (border.Child is TextBlock tb)
            {
                tb.Foreground = new SolidColorBrush(Colors.White);
            }

            // === Particles ===
            if (_keyPositions.TryGetValue(key, out var pos))
            {
                _particles?.Emit(pos.X, pos.Y, 4);
            }
        }
    }

    public void ReleaseKey(string key)
    {
        key = key.ToUpper();
        _pressedKeys.Remove(key);

        if (_keys.TryGetValue(key, out var border))
        {
            // === Smooth fade back to glass ===
            var glassBrush = new SolidColorBrush(Color.FromArgb(45, 255, 255, 255));
            border.Background = glassBrush;

            var fadeColor = new ColorAnimation
            {
                To = Color.FromArgb(25, 255, 255, 255),
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            glassBrush.BeginAnimation(SolidColorBrush.ColorProperty, fadeColor);

            // === Fade out glow ===
            if (border.Effect is DropShadowEffect glow)
            {
                var fadeGlow = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(350),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                fadeGlow.Completed += (_, _) => border.Effect = null;
                glow.BeginAnimation(DropShadowEffect.OpacityProperty, fadeGlow);
            }

            // === Restore border ===
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));

            // === Text dim back ===
            if (border.Child is TextBlock tb)
            {
                tb.Foreground = new SolidColorBrush(Color.FromArgb(210, 255, 255, 255));
            }

            // === Scale bounce back ===
            var scale = border.RenderTransform as ScaleTransform ?? new ScaleTransform(1.0, 1.0);
            border.RenderTransform = scale;

            var releaseAnim = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new ElasticEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Oscillations = 1,
                    Springiness = 8
                }
            };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, releaseAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, releaseAnim.Clone());
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