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
        using var hook = new KeyboardHook();

        hook.KeyPressed += (character) =>
        {
            window.Dispatcher.Invoke(() => window.SpawnLetter(character));
        };

        hook.Install();
        app.Run(window);
    }
}