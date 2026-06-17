using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TypeAestetic.Main;

using Application = System.Windows.Application;
using Color = System.Drawing.Color;

namespace TypeAestetic.View;

/// <summary>
/// System tray icon with context menu for controlling the app.
/// </summary>
public class TrayIcon : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly AppSettings _settings;
    private readonly SoundManager _soundManager;
    private readonly System.Windows.Application _app;
    private SettingsWindow? _settingsWindow;

    public event Action? ToggleOverlayRequested;

    public TrayIcon(AppSettings settings, SoundManager soundManager, System.Windows.Application app)
    {
        _settings = settings;
        _soundManager = soundManager;
        _app = app;

        _icon = new NotifyIcon
        {
            Text = "TypeAestetic",
            Visible = true,
            Icon = CreateDefaultIcon()
        };

        _icon.DoubleClick += (_, _) =>
        {
            _settings.OverlayVisible = !_settings.OverlayVisible;
            ToggleOverlayRequested?.Invoke();
        };

        BuildContextMenu();
    }

    private void BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());

        // Show/Hide overlay
        var toggleItem = new ToolStripMenuItem("Show/Hide Overlay");
        toggleItem.Click += (_, _) =>
        {
            _settings.OverlayVisible = !_settings.OverlayVisible;
            ToggleOverlayRequested?.Invoke();
        };
        menu.Items.Add(toggleItem);

        menu.Items.Add(new ToolStripSeparator());

        // Sound packs submenu
        var packsMenu = new ToolStripMenuItem("Sound Pack");
        RefreshPacksList(packsMenu);
        menu.Items.Add(packsMenu);

        // Toggle sound
        var soundToggle = new ToolStripMenuItem("Sound Enabled")
        {
            Checked = _settings.SoundEnabled,
            CheckOnClick = true
        };
        soundToggle.CheckedChanged += (_, _) =>
        {
            _settings.SoundEnabled = soundToggle.Checked;
            _soundManager.Enabled = soundToggle.Checked;
        };
        menu.Items.Add(soundToggle);

        menu.Items.Add(new ToolStripSeparator());

        // Settings
        var settingsItem = new ToolStripMenuItem("Settings...");
        settingsItem.Click += (_, _) => OpenSettings();
        menu.Items.Add(settingsItem);

        menu.Items.Add(new ToolStripSeparator());

        // Exit
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) =>
        {
            _icon.Visible = false;
            _app.Dispatcher.Invoke(() => _app.Shutdown());
        };
        menu.Items.Add(exitItem);

        _icon.ContextMenuStrip = menu;
    }

    private void RefreshPacksList(ToolStripMenuItem parent)
    {
        parent.DropDownItems.Clear();
        var packs = AppSettings.DiscoverSoundPacks();

        foreach (var pack in packs)
        {
            var item = new ToolStripMenuItem(pack)
            {
                Checked = (pack == _settings.SoundPack)
            };
            item.Click += (_, _) =>
            {
                _settings.SoundPack = pack;
                _soundManager.SwitchPack(pack);
                // Update check marks
                foreach (ToolStripMenuItem child in parent.DropDownItems)
                    child.Checked = (child.Text == pack);
            };
            parent.DropDownItems.Add(item);
        }

        if (packs.Length == 0)
        {
            parent.DropDownItems.Add(new ToolStripMenuItem("(no packs found)") { Enabled = false });
        }
    }

    private void OpenSettings()
    {
        _app.Dispatcher.Invoke(() =>
        {
            if (_settingsWindow == null || !_settingsWindow.IsLoaded)
            {
                _settingsWindow = new SettingsWindow(_settings, _soundManager);
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
            }
        });
    }

    private static Icon CreateDefaultIcon()
    {
        // Create a simple colored icon programmatically
        var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(System.Drawing.Color.Transparent);
            using var brush = new SolidBrush(System.Drawing.Color.FromArgb(0, 229, 255));
            g.FillRectangle(brush, 2, 2, 12, 12);
            using var borderPen = new Pen(System.Drawing.Color.FromArgb(0, 180, 220), 1);
            g.DrawRectangle(borderPen, 2, 2, 11, 11);
            // Small "T" letter
            using var font = new Font("Segoe UI", 8f, FontStyle.Bold);
            g.DrawString("T", font, System.Drawing.Brushes.White, 2, 1);
        }
        return System.Drawing.Icon.FromHandle(bmp.GetHicon());
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }

    /// <summary>
    /// Dark theme for the context menu
    /// </summary>
    private class DarkColorTable : ProfessionalColorTable
    {
        public override System.Drawing.Color MenuBorder => System.Drawing.Color.FromArgb(60, 60, 60);
        public override System.Drawing.Color MenuItemSelected => System.Drawing.Color.FromArgb(50, 50, 55);
        public override System.Drawing.Color MenuItemBorder => System.Drawing.Color.FromArgb(0, 200, 230);
        public override System.Drawing.Color MenuStripGradientBegin => System.Drawing.Color.FromArgb(30, 30, 34);
        public override System.Drawing.Color MenuStripGradientEnd => System.Drawing.Color.FromArgb(30, 30, 34);
        public override System.Drawing.Color MenuItemSelectedGradientBegin => System.Drawing.Color.FromArgb(45, 45, 50);
        public override System.Drawing.Color MenuItemSelectedGradientEnd => System.Drawing.Color.FromArgb(45, 45, 50);
        public override System.Drawing.Color MenuItemPressedGradientBegin => System.Drawing.Color.FromArgb(0, 180, 220);
        public override System.Drawing.Color MenuItemPressedGradientEnd => System.Drawing.Color.FromArgb(0, 150, 200);
        public override System.Drawing.Color ImageMarginGradientBegin => System.Drawing.Color.FromArgb(35, 35, 38);
        public override System.Drawing.Color ImageMarginGradientMiddle => System.Drawing.Color.FromArgb(35, 35, 38);
        public override System.Drawing.Color ImageMarginGradientEnd => System.Drawing.Color.FromArgb(35, 35, 38);
        public override System.Drawing.Color ToolStripDropDownBackground => System.Drawing.Color.FromArgb(35, 35, 38);
        public override System.Drawing.Color SeparatorDark => System.Drawing.Color.FromArgb(55, 55, 60);
        public override System.Drawing.Color SeparatorLight => System.Drawing.Color.FromArgb(55, 55, 60);
    }
}
