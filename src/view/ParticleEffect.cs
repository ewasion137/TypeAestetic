using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace TypeAestetic.View;

/// <summary>
/// Неоновые искры при ударах по клавишам.
/// </summary>
public class ParticleEffect
{
    private readonly Canvas _canvas;
    private readonly Random _rng = new();
    public bool Enabled { get; set; } = true;
    public Color AccentColor { get; set; } = Color.FromRgb(0, 229, 255);

    public ParticleEffect(Canvas canvas) => _canvas = canvas;

    public void Emit(double cx, double cy, int count = 6)
    {
        if (!Enabled) return;
        for (int i = 0; i < count; i++)
            SpawnSpark(cx, cy);
    }

    private void SpawnSpark(double cx, double cy)
    {
        double size = 2.5 + _rng.NextDouble() * 3.5;
        double angle = _rng.NextDouble() * Math.PI * 2;
        double speed = 25 + _rng.NextDouble() * 45;
        double duration = 250 + _rng.NextDouble() * 250;

        var spark = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new RadialGradientBrush(
                Colors.White,
                Color.FromArgb(0, AccentColor.R, AccentColor.G, AccentColor.B))
        };

        Canvas.SetLeft(spark, cx - size / 2);
        Canvas.SetTop(spark, cy - size / 2);
        _canvas.Children.Add(spark);

        var dur = TimeSpan.FromMilliseconds(duration);
        var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };

        var moveX = new DoubleAnimation { By = Math.Cos(angle) * speed, Duration = dur, EasingFunction = ease };
        var moveY = new DoubleAnimation { By = Math.Sin(angle) * speed - 18, Duration = dur, EasingFunction = ease };

        var fade = new DoubleAnimation { From = 1.0, To = 0.0, Duration = dur, EasingFunction = ease };
        var scale = new ScaleTransform(1.0, 1.0);
        spark.RenderTransform = scale;
        spark.RenderTransformOrigin = new Point(0.5, 0.5);

        var shrink = new DoubleAnimation { To = 0.1, Duration = dur };

        fade.Completed += (_, _) => _canvas.Children.Remove(spark);

        spark.BeginAnimation(Canvas.LeftProperty, moveX);
        spark.BeginAnimation(Canvas.TopProperty, moveY);
        spark.BeginAnimation(UIElement.OpacityProperty, fade);
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, shrink);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, shrink);
    }
}