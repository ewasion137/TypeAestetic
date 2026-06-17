using System;
using System.IO;
using NAudio.Wave;

namespace TypeAestetic.Main;

public class SoundManager
{
    private readonly string _soundDir;

    public SoundManager(string packFolder)
    {
        // Path to your wav files
        _soundDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "sounds", packFolder);
    }

    public void Play(string key, bool isClick)
    {
        // Simple logic: key_click.wav or key_release.wav
        // Fallback to generic_click.wav if specific not found
        string type = isClick ? "click" : "release";
        string fileName = $"{key.ToLower()}_{type}.wav";
        string fullPath = Path.Combine(_soundDir, fileName);

        if (!File.Exists(fullPath))
        {
            fullPath = Path.Combine(_soundDir, $"generic_{type}.wav");
        }

        if (File.Exists(fullPath))
        {
            PlayFile(fullPath);
        }
    }

    private void PlayFile(string path)
    {
        // Fire and forget audio playback
        var reader = new AudioFileReader(path);
        var output = new WaveOutEvent();
        output.Init(reader);
        output.Play();

        // Cleanup when done
        output.PlaybackStopped += (s, e) => {
            output.Dispose();
            reader.Dispose();
        };
    }
}