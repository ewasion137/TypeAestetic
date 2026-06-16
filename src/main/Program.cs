using System;
using System.Windows;
using TypeAestetic.View;

namespace TypeAestetic.Main
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            var window = new OverlayWindow();
            app.Run(window);
        }
    }
}