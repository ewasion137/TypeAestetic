using System;
using System.Windows;
using TypeAestetic.Hooks;
using TypeAestetic.View;

namespace TypeAestetic.Main;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        System.Windows.Application app = new System.Windows.Application();
    app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Load settings
        var settings = AppSettings.Load();

        // Initialize components
        var window = new OverlayWindow(settings);
        var view = new KeyboardView();
        var sound = new SoundManager(settings.SoundPack)
        {
            Volume = settings.Volume,
            Enabled = settings.SoundEnabled
        };
        using var hook = new KeyboardHook();

        // Configure view from settings
        view.SetParticlesEnabled(settings.ParticlesEnabled);

        // React to settings changes
        settings.PropertyChanged += (_, e) =>
        {
            window.Dispatcher.Invoke(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(AppSettings.ParticlesEnabled):
                        view.SetParticlesEnabled(settings.ParticlesEnabled);
                        break;
                    case nameof(AppSettings.Volume):
                        sound.Volume = settings.Volume;
                        break;
                    case nameof(AppSettings.SoundEnabled):
                        sound.Enabled = settings.SoundEnabled;
                        break;
                    case nameof(AppSettings.SoundPack):
                        sound.SwitchPack(settings.SoundPack);
                        break;
                }
            });
        };

        window.Content = view;

        // System tray icon
        using var tray = new TrayIcon(settings, sound, app);
        tray.ToggleOverlayRequested += () =>
        {
            window.Dispatcher.Invoke(() =>
            {
                window.ToggleVisibility(settings.OverlayVisible);
            });
        };

        // Keyboard events
        hook.KeyPressed += (key) =>
        {
            window.Dispatcher.Invoke(() =>
            {
                view.PressKey(key);
                sound.Play(key, true);
            });
        };

        hook.KeyReleased += (key) =>
        {
            window.Dispatcher.Invoke(() =>
            {
                view.ReleaseKey(key);
                sound.Play(key, false);
            });
        };

        hook.Install();

        // Clean shutdown
        app.Exit += (_, _) =>
        {
            hook.Dispose();
            sound.Dispose();
            tray.Dispose();
        };

        app.Run(window);
    }
}