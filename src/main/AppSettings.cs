using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypeAestetic.Main;

public class AppSettings : INotifyPropertyChanged
{
    private static readonly string SettingsPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Backing fields
    private double _overlayOpacity = 0.92;
    private OverlayCorner _overlayCorner = OverlayCorner.BottomRight;
    private bool _particlesEnabled = true;
    private string _accentColor = "#00E5FF"; // Electric cyan
    private bool _statsEnabled = true;
    private bool _overlayVisible = true;

    public double OverlayOpacity
    {
        get => _overlayOpacity;
        set { if (_overlayOpacity != value) { _overlayOpacity = Math.Clamp(value, 0.1, 1.0); OnPropertyChanged(nameof(OverlayOpacity)); Save(); } }
    }

    public OverlayCorner OverlayCorner
    {
        get => _overlayCorner;
        set { if (_overlayCorner != value) { _overlayCorner = value; OnPropertyChanged(nameof(OverlayCorner)); Save(); } }
    }

    public bool ParticlesEnabled
    {
        get => _particlesEnabled;
        set { if (_particlesEnabled != value) { _particlesEnabled = value; OnPropertyChanged(nameof(ParticlesEnabled)); Save(); } }
    }

    public string AccentColor
    {
        get => _accentColor;
        set { if (_accentColor != value) { _accentColor = value; OnPropertyChanged(nameof(AccentColor)); Save(); } }
    }

    public bool StatsEnabled
    {
        get => _statsEnabled;
        set { if (_statsEnabled != value) { _statsEnabled = value; OnPropertyChanged(nameof(StatsEnabled)); Save(); } }
    }

    [JsonIgnore]
    public bool OverlayVisible
    {
        get => _overlayVisible;
        set { if (_overlayVisible != value) { _overlayVisible = value; OnPropertyChanged(nameof(OverlayVisible)); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Settings] Save failed: {ex.Message}");
        }
    }

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Settings] Load failed: {ex.Message}");
        }
        return new AppSettings();
    }
}

public enum OverlayCorner
{
    BottomRight,
    BottomLeft,
    TopRight,
    TopLeft
}