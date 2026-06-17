using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NAudio.Wave;
using System.Threading.Tasks;

namespace TypeAestetic.Main;

public class SoundManager
{
    private readonly string _packPath;
    private float _volume = 1.0f;
    private Dictionary<string, (string click, string release)> _mappings = new();

    public SoundManager(string folderName)
    {
        // Root path where assets live
        _packPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "sounds", folderName);

        EnsureDirectoryExists();
        LoadConfig();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_packPath))
        {
            Directory.CreateDirectory(_packPath);
            Console.WriteLine($"[INFO] Created missing directory: {_packPath}");
            // Here you could create a template config.json if needed
        }
    }

    private void LoadConfig()
    {
        string configPath = Path.Combine(_packPath, "config.json");
        Console.WriteLine($"[DEBUG] Full Config Path: {Path.GetFullPath(configPath)}");

        if (!File.Exists(configPath))
        {
            Console.WriteLine($"[!!!] CONFIG NOT FOUND at {configPath}");
            return;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Settings
            if (root.TryGetProperty("settings", out var settings))
                _volume = (float)settings.GetProperty("volume").GetDouble();

            Console.WriteLine($"[DEBUG] Volume set to: {_volume}");

            // Maps
            var maps = root.GetProperty("maps");
            foreach (var property in maps.EnumerateObject())
            {
                var click = property.Value.GetProperty("click").GetString()!;
                var release = property.Value.GetProperty("release").GetString()!;
                _mappings[property.Name.ToUpper()] = (click, release);
                Console.WriteLine($"[DEBUG] Mapped: {property.Name} -> {click} / {release}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FATAL] JSON Error: {ex.Message}");
        }
    }

    // [WHERE: Play Method]
    public void Play(string key, bool isClick)
    {
        string normKey = NormalizeKeyName(key);
        Console.WriteLine($"[EVENT] Key: {key} (Normalized: {normKey}), Click: {isClick}");

        if (!_mappings.TryGetValue(normKey, out var files))
        {
            if (!_mappings.TryGetValue("DEFAULT", out files))
            {
                Console.WriteLine($"[WARN] No mapping for {normKey} and no DEFAULT found.");
                return;
            }
        }

        string fileName = isClick ? files.click : files.release;
        string fullPath = Path.Combine(_packPath, fileName);

        if (File.Exists(fullPath))
        {
            PlayFile(fullPath);
        }
        else
        {
            Console.WriteLine($"[!!!] WAV MISSING: {fullPath}");
        }
    }

    // [WHERE: PlayFile Method]
    private void PlayFile(string path)
    {
        Task.Run(() =>
        {
            try
            {
                using var reader = new AudioFileReader(path) { Volume = _volume };
                using var output = new WaveOutEvent();
                output.Init(reader);
                output.Play();
                Console.WriteLine($"[AUDIO] Playing: {Path.GetFileName(path)}");

                while (output.PlaybackState == PlaybackState.Playing)
                {
                    System.Threading.Thread.Sleep(5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUDIO ERROR] Failed to play {path}: {ex.Message}");
            }
        });
    }

    private string NormalizeKeyName(string key)
    {
        key = key.ToUpper();
        if (key.Contains("SHIFT")) return "SHIFT";
        if (key == "SPACE") return "SPACE";
        if (key == "RETURN") return "RETURN";
        if (key == "BACK") return "BACK";
        return key;
    }
}