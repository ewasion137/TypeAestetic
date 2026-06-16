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
        hook.KeyPressed += (key) => 
        {  
            window.Dispatcher.Invoke(() => view.PressKey(key.ToString()));
        };

        hook.KeyReleased += (key) => 
        {     
            window.Dispatcher.Invoke(() => view.ReleaseKey(key.ToString()));
        };

        hook.Install();
        app.Run(window);
    }
}