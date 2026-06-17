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
        using var hook = new KeyboardHook();
        // Initialize SoundManager with your folder name
        var sound = new SoundManager("b865");

        window.Content = view;

        // [WHERE: KeyPressed event]
        hook.KeyPressed += (key) =>
        {
            Console.WriteLine($"[HOOK] Key Down: {key}"); // Add this
            window.Dispatcher.Invoke(() =>
            {
                view.PressKey(key);
                sound.Play(key, true);
            });
        };

        // [WHERE: Inside Main KeyReleased]
        hook.KeyReleased += (key) =>
        {
            Console.WriteLine($"[HOOK] Key Up: {key}"); // Add this
            window.Dispatcher.Invoke(() =>
            {
                view.ReleaseKey(key);
                sound.Play(key, false);
            });
        };

        hook.Install();
        app.Run(window);
    }
}