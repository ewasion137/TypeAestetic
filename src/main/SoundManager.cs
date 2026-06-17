using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NAudio.Wave;

namespace TypeAestetic.Main;

public class SoundManager
{
    private readonly string _packPath;
    private Dictionary<string, JsonElement>? _config;
    private readonly Random _rng = new();

    public SoundManager(string packName)
    {
        _packPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "soundpacks", packName);
        LoadConfig();
    }

    private void LoadConfig()
    {
        var json = File.ReadAllText(Path.Combine(_packPath, "config.json"));
        var doc = JsonDocument.Parse(json);
        _config = doc.RootElement.GetProperty("maps").EnumerateObject().ToDictionary(x => x.Name, x => x.Value);
    }

    public void Play(string key, bool isClick)
    {
        string keyUpper = key.ToUpper();
        if (_config == null) return;

        // Ищем в конфиге конкретную клавишу или DEFAULT
        if (!_config.TryGetValue(keyUpper, out var keyMap))
            keyMap = _config["DEFAULT"];

        string fileName = keyMap.GetProperty(isClick ? "click" : "release").GetString()!;
        string fullPath = Path.Combine(_packPath, fileName);

        if (File.Exists(fullPath))
        {
            // Проигрывание через NAudio (Fire and forget)
            var reader = new AudioFileReader(fullPath);
            var output = new WaveOutEvent();
            output.Init(reader);
            output.Play();

            // Чистим память после завершения
            output.PlaybackStopped += (s, e) =>
            {
                output.Dispose();
                reader.Dispose();
            };
        }
    }
}