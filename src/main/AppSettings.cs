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
    private float _volume = 0.8f;
    private string _soundPack = "b865";
    private double _overlayOpacity = 0.92;
    private OverlayCorner _overlayCorner = OverlayCorner.BottomRight;
    private bool _particlesEnabled = true;
    private string _accentColor = "#00E5FF"; // Electric cyan
    private bool _statsEnabled = true;
    private bool _soundEnabled = true;
    private bool _overlayVisible = true;

    public float Volume
    {
        get => _volume;
        set { if (_volume != value) { _volume = Math.Clamp(value, 0f, 1f); OnPropertyChanged(nameof(Volume)); Save(); } }
    }

    public string SoundPack
    {
        get => _soundPack;
        set { if (_soundPack != value) { _soundPack = value; OnPropertyChanged(nameof(SoundPack)); Save(); } }
    }

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

    public bool SoundEnabled
    {
        get => _soundEnabled;
        set { if (_soundEnabled != value) { _soundEnabled = value; OnPropertyChanged(nameof(SoundEnabled)); Save(); } }
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

    /// <summary>
    /// Returns all available sound pack folder names from assets/sounds/
    /// </summary>
    public static string[] DiscoverSoundPacks()
    {
        var soundsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "sounds");
        if (!Directory.Exists(soundsDir))
            return Array.Empty<string>();

        return Directory.GetDirectories(soundsDir)
            .Select(Path.GetFileName)
            .Where(name => name != null)
            .ToArray()!;
    }
}

public enum OverlayCorner
{
    BottomRight,
    BottomLeft,
    TopRight,
    TopLeft
}
