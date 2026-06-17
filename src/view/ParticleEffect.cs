using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;

namespace TypeAestetic.View;

using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;

/// <summary>
/// Lightweight particle effect system for key press sparkles.
/// Renders on a Canvas overlay — particles auto-remove after animation.
/// </summary>
public class ParticleEffect
{
    private readonly Canvas _canvas;
    private readonly Random _rng = new();
    private bool _enabled = true;

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    public Color AccentColor { get; set; } = Colors.Cyan;

    public ParticleEffect(Canvas canvas)
    {
        _canvas = canvas;
    }

    /// <summary>
    /// Emit a burst of sparkle particles from the given screen position.
    /// </summary>
    public void Emit(double centerX, double centerY, int count = 5)
    {
        if (!_enabled) return;

        for (int i = 0; i < count; i++)
        {
            SpawnParticle(centerX, centerY);
        }
    }

    private void SpawnParticle(double cx, double cy)
    {
        double size = 2 + _rng.NextDouble() * 3; // 2–5px
        double angle = _rng.NextDouble() * Math.PI * 2;
        double speed = 20 + _rng.NextDouble() * 40; // 20–60px travel
        double duration = 300 + _rng.NextDouble() * 400; // 300–700ms

        // Randomize color slightly around the accent
        byte r = (byte)Math.Clamp(AccentColor.R + _rng.Next(-20, 20), 0, 255);
        byte g = (byte)Math.Clamp(AccentColor.G + _rng.Next(-20, 20), 0, 255);
        byte b = (byte)Math.Clamp(AccentColor.B + _rng.Next(-20, 20), 0, 255);
        var color = Color.FromArgb(220, r, g, b);

        var particle = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new RadialGradientBrush(
                Color.FromArgb(255, r, g, b),
                Color.FromArgb(0, r, g, b))
        };

        Canvas.SetLeft(particle, cx - size / 2);
        Canvas.SetTop(particle, cy - size / 2);
        _canvas.Children.Add(particle);

        // Animate position
        double dx = Math.Cos(angle) * speed;
        double dy = Math.Sin(angle) * speed - 15; // Slight upward bias

        var dur = TimeSpan.FromMilliseconds(duration);
        var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };

        var moveX = new DoubleAnimation
        {
            By = dx,
            Duration = dur,
            EasingFunction = ease
        };

        var moveY = new DoubleAnimation
        {
            By = dy,
            Duration = dur,
            EasingFunction = ease
        };

        // Fade out
        var fadeOut = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = dur,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        // Scale down
        var scale = new ScaleTransform(1.0, 1.0);
        particle.RenderTransform = scale;
        particle.RenderTransformOrigin = new Point(0.5, 0.5);

        var shrink = new DoubleAnimation
        {
            To = 0.0,
            Duration = dur,
            EasingFunction = ease
        };

        // Remove particle when animation completes
        fadeOut.Completed += (_, _) =>
        {
            _canvas.Children.Remove(particle);
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveX);
        particle.BeginAnimation(Canvas.TopProperty, moveY);
        particle.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, shrink);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, shrink);
    }
}
