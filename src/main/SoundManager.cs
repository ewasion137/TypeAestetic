using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace TypeAestetic.Main;

/// <summary>
/// High-performance sound manager with pre-cached audio buffers,
/// a pool of output devices, and pitch variation.
/// </summary>
public class SoundManager : IDisposable
{
    private string _packPath;
    private float _volume = 1.0f;
    private float _pitchVariation = 0.05f;
    private Dictionary<string, (string click, string release)> _mappings = new();
    private readonly ConcurrentDictionary<string, byte[]> _audioCache = new();
    private readonly Random _rng = new();

    // Pool of output devices for overlapping playback
    private const int PoolSize = 12;
    private readonly WaveOutEvent[] _outputPool;
    private int _poolIndex;
    private readonly object _poolLock = new();

    private bool _disposed;

    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0f, 1f);
    }

    public bool Enabled { get; set; } = true;

    public string CurrentPack { get; private set; }

    public SoundManager(string folderName)
    {
        CurrentPack = folderName;
        _packPath = GetPackPath(folderName);

        // Initialize output pool
        _outputPool = new WaveOutEvent[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            _outputPool[i] = new WaveOutEvent
            {
                DesiredLatency = 50 // Low latency for responsiveness
            };
        }

        EnsureDirectoryExists();
        LoadConfig();
        PreCacheAudio();
    }

    private static string GetPackPath(string folderName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "sounds", folderName);
    }

    /// <summary>
    /// Hot-switch to a different sound pack without restarting.
    /// </summary>
    public void SwitchPack(string folderName)
    {
        CurrentPack = folderName;
        _packPath = GetPackPath(folderName);
        _audioCache.Clear();
        _mappings.Clear();
        EnsureDirectoryExists();
        LoadConfig();
        PreCacheAudio();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_packPath))
        {
            Directory.CreateDirectory(_packPath);
            System.Diagnostics.Debug.WriteLine($"[SoundManager] Created missing directory: {_packPath}");
        }
    }

    private void LoadConfig()
    {
        string configPath = Path.Combine(_packPath, "config.json");

        if (!File.Exists(configPath))
        {
            System.Diagnostics.Debug.WriteLine($"[SoundManager] Config not found at {configPath}");
            return;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("settings", out var settings))
            {
                if (settings.TryGetProperty("volume", out var vol))
                    _volume = (float)vol.GetDouble();
                if (settings.TryGetProperty("pitch_variation", out var pitch))
                    _pitchVariation = (float)pitch.GetDouble();
            }

            if (root.TryGetProperty("maps", out var maps))
            {
                foreach (var property in maps.EnumerateObject())
                {
                    var click = property.Value.GetProperty("click").GetString()!;
                    var release = property.Value.GetProperty("release").GetString()!;
                    _mappings[property.Name.ToUpper()] = (click, release);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SoundManager] Config error: {ex.Message}");
        }
    }

    /// <summary>
    /// Load all referenced WAV files into memory buffers at startup.
    /// Files are tiny (2–25KB) so this is fast and eliminates disk I/O during typing.
    /// </summary>
    private void PreCacheAudio()
    {
        foreach (var (_, files) in _mappings)
        {
            CacheFile(files.click);
            CacheFile(files.release);
        }
    }

    private void CacheFile(string fileName)
    {
        if (_audioCache.ContainsKey(fileName)) return;

        var fullPath = Path.Combine(_packPath, fileName);
        if (File.Exists(fullPath))
        {
            _audioCache[fileName] = File.ReadAllBytes(fullPath);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[SoundManager] Missing audio file: {fullPath}");
        }
    }

    public void Play(string key, bool isClick)
    {
        if (!Enabled) return;

        string normKey = NormalizeKeyName(key);

        if (!_mappings.TryGetValue(normKey, out var files))
        {
            if (!_mappings.TryGetValue("DEFAULT", out files))
                return;
        }

        string fileName = isClick ? files.click : files.release;

        if (!_audioCache.TryGetValue(fileName, out var audioData))
            return;

        PlayFromBuffer(audioData);
    }

    private void PlayFromBuffer(byte[] audioData)
    {
        WaveOutEvent output;

        lock (_poolLock)
        {
            output = _outputPool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % PoolSize;
        }

        try
        {
            // Stop any currently playing sound on this channel
            if (output.PlaybackState == PlaybackState.Playing)
                output.Stop();

            var stream = new MemoryStream(audioData);
            var reader = new WaveFileReader(stream);

            // Convert to sample provider for pitch manipulation
            var sampleProvider = reader.ToSampleProvider();

            // Apply pitch variation — subtle randomization for realism
            ISampleProvider finalProvider;
            if (_pitchVariation > 0.001f)
            {
                // Use resampling for pitch shift: change the playback rate slightly
                float pitchFactor = 1.0f + (_rng.NextSingle() * 2 - 1) * _pitchVariation;
                var resampled = new WdlResamplingSampleProvider(sampleProvider,
                    (int)(sampleProvider.WaveFormat.SampleRate * pitchFactor));
                finalProvider = resampled;
            }
            else
            {
                finalProvider = sampleProvider;
            }

            // Apply volume
            var volumeProvider = new VolumeSampleProvider(finalProvider)
            {
                Volume = _volume
            };

            output.Init(volumeProvider);
            output.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SoundManager] Playback error: {ex.Message}");
        }
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var output in _outputPool)
        {
            try
            {
                output.Stop();
                output.Dispose();
            }
            catch { }
        }

        GC.SuppressFinalize(this);
    }
}