using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NAudio.Wave;

namespace TypeAestetic.Main;

public class SoundManager
{
    private readonly string _packPath;
    private float _volume = 1.0f;
    private Dictionary<string, (string click, string release)> _mappings = new();

    public SoundManager(string packName)
    {
        // Path logic: looks into /assets/soundpacks/name/
        _packPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "sounds", packName);
        LoadConfig();
    }

    private void LoadConfig()
    {
        string configPath = Path.Combine(_packPath, "config.json");
        if (!File.Exists(configPath)) return;

        var json = File.ReadAllText(configPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Load Global Volume
        if (root.TryGetProperty("settings", out var settings))
            _volume = (float)settings.GetProperty("volume").GetDouble();

        // Load Key Mappings
        var maps = root.GetProperty("maps");
        foreach (var property in maps.EnumerateObject())
        {
            _mappings[property.Name.ToUpper()] = (
                property.Value.GetProperty("click").GetString()!,
                property.Value.GetProperty("release").GetString()!
            );
        }
    }

    public void Play(string key, bool isClick)
    {
        key = NormalizeKeyName(key);
        
        // Find mapping or use DEFAULT
        if (!_mappings.TryGetValue(key, out var files))
            if (!_mappings.TryGetValue("DEFAULT", out files)) return;

        string fileName = isClick ? files.click : files.release;
        string fullPath = Path.Combine(_packPath, fileName);

        if (File.Exists(fullPath)) PlayFile(fullPath);
    }

    private string NormalizeKeyName(string key)
    {
        key = key.ToUpper();
        // Map system names (LEFTSHIFT) to your config names (SHIFT)
        if (key.Contains("SHIFT")) return "SHIFT";
        if (key.Contains("CTRL")) return "CTRL"; // Added if you add it later
        if (key == "SPACE") return "SPACE";
        if (key == "RETURN") return "RETURN";
        if (key == "BACK") return "BACK";
        return key;
    }

    private void PlayFile(string path)
    {
        var reader = new AudioFileReader(path) { Volume = _volume };
        var output = new WaveOutEvent();
        output.Init(reader);
        output.Play();

        output.PlaybackStopped += (s, e) => {
            output.Dispose();
            reader.Dispose();
        };
    }
}