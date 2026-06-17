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
        var app = new Application();
        var window = new OverlayWindow();
        var view = new KeyboardView();

        // Initialize SoundManager with your folder name
        var sound = new SoundManager("b865");

        window.Content = view;

        using var hook = new KeyboardHook();

        hook.KeyPressed += (key) =>
        {
            window.Dispatcher.Invoke(() =>
            {
                view.PressKey(key);
                sound.Play(key, true); // Key Down Sound
            });
        };

        hook.KeyReleased += (key) =>
        {
            window.Dispatcher.Invoke(() =>
            {
                view.ReleaseKey(key);
                sound.Play(key, false); // Key Up Sound
            });
        };

        hook.Install();
        app.Run(window);
    }
}