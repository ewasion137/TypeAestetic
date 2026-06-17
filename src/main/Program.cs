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

        window.Content = view;

        using var hook = new KeyboardHook();
        var soundManager = new SoundManager("bloody_red");

        hook.KeyPressed += (keyName) =>
        {
            window.Dispatcher.Invoke(() =>
            {
                view.PressKey(keyName);
                soundManager.Play(keyName, true); // Play Click
            });
        };

        hook.KeyReleased += (keyName) =>
        {
            window.Dispatcher.Invoke(() =>
            {
                view.ReleaseKey(keyName);
                soundManager.Play(keyName, false); // Play Release
            });
        };

        hook.Install();
        app.Run(window);
    }
}