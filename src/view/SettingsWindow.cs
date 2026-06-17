using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using TypeAestetic.Main;

// Add this alias to resolve ambiguity
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using ComboBox = System.Windows.Controls.ComboBox;
using CheckBox = System.Windows.Controls.CheckBox;
using Orientation = System.Windows.Controls.Orientation;
using TextBlock = System.Windows.Controls.TextBlock;

namespace TypeAestetic.View;

/// <summary>
/// Styled settings window matching the glassmorphism aesthetic.
/// </summary>
public class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly SoundManager _soundManager;

    public SettingsWindow(AppSettings settings, SoundManager soundManager)
    {
        _settings = settings;
        _soundManager = soundManager;

        Title = "TypeAestetic — Settings";
        Width = 420;
        Height = 520;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;

        BuildUI();
    }

    private void BuildUI()
    {
        // Outer container with shadow
        var outerBorder = new Border
        {
            Margin = new Thickness(16),
            CornerRadius = new CornerRadius(16),
            Background = new SolidColorBrush(Color.FromRgb(22, 22, 28)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect
            {
                BlurRadius = 30,
                ShadowDepth = 0,
                Opacity = 0.5,
                Color = Colors.Black
            }
        };

        var mainStack = new StackPanel { Margin = new Thickness(24) };

        // === Title bar ===
        var titleBar = new Grid { Margin = new Thickness(0, 0, 0, 20) };
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

        var title = new TextBlock
        {
            Text = "⚙ Settings",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 229, 255)),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(title, 0);
        titleBar.Children.Add(title);

        var closeBtn = CreateCloseButton();
        Grid.SetColumn(closeBtn, 1);
        titleBar.Children.Add(closeBtn);

        mainStack.Children.Add(titleBar);

        // === Volume ===
        mainStack.Children.Add(CreateSection("Volume", () =>
        {
            var slider = CreateStyledSlider(0, 100, _settings.Volume * 100);
            var label = CreateValueLabel($"{(int)(_settings.Volume * 100)}%");
            slider.ValueChanged += (_, e) =>
            {
                _settings.Volume = (float)(e.NewValue / 100.0);
                _soundManager.Volume = _settings.Volume;
                label.Text = $"{(int)e.NewValue}%";
            };
            var panel = new DockPanel();
            DockPanel.SetDock(label, Dock.Right);
            panel.Children.Add(label);
            panel.Children.Add(slider);
            return panel;
        }));

        // === Overlay Opacity ===
        mainStack.Children.Add(CreateSection("Overlay Opacity", () =>
        {
            var slider = CreateStyledSlider(10, 100, _settings.OverlayOpacity * 100);
            var label = CreateValueLabel($"{(int)(_settings.OverlayOpacity * 100)}%");
            slider.ValueChanged += (_, e) =>
            {
                _settings.OverlayOpacity = e.NewValue / 100.0;
                label.Text = $"{(int)e.NewValue}%";
            };
            var panel = new DockPanel();
            DockPanel.SetDock(label, Dock.Right);
            panel.Children.Add(label);
            panel.Children.Add(slider);
            return panel;
        }));

        // === Sound Pack ===
        mainStack.Children.Add(CreateSection("Sound Pack", () =>
        {
            var combo = new ComboBox
            {
                Background = new SolidColorBrush(Color.FromRgb(35, 35, 42)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                FontSize = 13,
                Padding = new Thickness(8, 6, 8, 6)
            };

            var packs = AppSettings.DiscoverSoundPacks();
            foreach (var pack in packs)
            {
                combo.Items.Add(pack);
                if (pack == _settings.SoundPack)
                    combo.SelectedItem = pack;
            }

            combo.SelectionChanged += (_, _) =>
            {
                if (combo.SelectedItem is string pack)
                {
                    _settings.SoundPack = pack;
                    _soundManager.SwitchPack(pack);
                }
            };

            return combo;
        }));

        // === Overlay Corner ===
        mainStack.Children.Add(CreateSection("Overlay Position", () =>
        {
            var panel = new WrapPanel { Orientation = Orientation.Horizontal };
            var corners = new[] {
                ("↘ Bottom Right", OverlayCorner.BottomRight),
                ("↙ Bottom Left", OverlayCorner.BottomLeft),
                ("↗ Top Right", OverlayCorner.TopRight),
                ("↖ Top Left", OverlayCorner.TopLeft)
            };

            foreach (var (label, corner) in corners)
            {
                var btn = CreateCornerButton(label, corner == _settings.OverlayCorner);
                btn.Click += (_, _) =>
                {
                    _settings.OverlayCorner = corner;
                    // Update all button states
                    foreach (Button child in panel.Children)
                    {
                        child.Background = new SolidColorBrush(Color.FromRgb(35, 35, 42));
                        child.Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
                    }
                    btn.Background = new SolidColorBrush(Color.FromArgb(40, 0, 229, 255));
                    btn.Foreground = new SolidColorBrush(Color.FromRgb(0, 229, 255));
                };
                panel.Children.Add(btn);
            }

            return panel;
        }));

        // === Toggles ===
        mainStack.Children.Add(CreateToggleSection("Particle Effects", _settings.ParticlesEnabled,
            isOn => _settings.ParticlesEnabled = isOn));

        mainStack.Children.Add(CreateToggleSection("Typing Stats", _settings.StatsEnabled,
            isOn => _settings.StatsEnabled = isOn));

        mainStack.Children.Add(CreateToggleSection("Sound", _settings.SoundEnabled,
            isOn =>
            {
                _settings.SoundEnabled = isOn;
                _soundManager.Enabled = isOn;
            }));

        // Make window draggable from title area
        titleBar.MouseLeftButtonDown += (_, _) => DragMove();

        outerBorder.Child = mainStack;
        Content = outerBorder;
    }

    private Border CreateSection(string labelText, Func<UIElement> contentFactory)
    {
        var section = new Border
        {
            Margin = new Thickness(0, 0, 0, 14),
            Padding = new Thickness(0)
        };

        var stack = new StackPanel();

        var label = new TextBlock
        {
            Text = labelText,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(140, 255, 255, 255)),
            Margin = new Thickness(0, 0, 0, 6),
        };
        stack.Children.Add(label);
        stack.Children.Add(contentFactory());

        section.Child = stack;
        return section;
    }

    private Border CreateToggleSection(string labelText, bool initialValue, Action<bool> onChanged)
    {
        var section = new Border
        {
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(12, 8, 12, 8),
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 36))
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

        var label = new TextBlock
        {
            Text = labelText,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromArgb(210, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(label, 0);

        var toggle = new CheckBox
        {
            IsChecked = initialValue,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 229, 255))
        };
        toggle.Checked += (_, _) => onChanged(true);
        toggle.Unchecked += (_, _) => onChanged(false);
        Grid.SetColumn(toggle, 1);

        grid.Children.Add(label);
        grid.Children.Add(toggle);

        section.Child = grid;
        return section;
    }

    private Slider CreateStyledSlider(double min, double max, double value)
    {
        return new Slider
        {
            Minimum = min,
            Maximum = max,
            Value = value,
            IsSnapToTickEnabled = true,
            TickFrequency = 1,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
    }

    private TextBlock CreateValueLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 229, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 40,
            TextAlignment = TextAlignment.Right
        };
    }

    private Button CreateCornerButton(string text, bool isSelected)
    {
        return new Button
        {
            Content = text,
            FontSize = 11,
            Padding = new Thickness(10, 6, 10, 6),
            Margin = new Thickness(0, 0, 6, 6),
            Background = isSelected
                ? new SolidColorBrush(Color.FromArgb(40, 0, 229, 255))
                : new SolidColorBrush(Color.FromRgb(35, 35, 42)),
            Foreground = isSelected
                ? new SolidColorBrush(Color.FromRgb(0, 229, 255))
                : new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            Cursor = System.Windows.Input.Cursors.Hand
        };
    }

    private Button CreateCloseButton()
    {
        var btn = new Button
        {
            Content = "✕",
            FontSize = 14,
            Width = 28,
            Height = 28,
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.FromArgb(140, 255, 255, 255)),
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        };
        btn.Click += (_, _) => Close();
        return btn;
    }
}
