using MusicPlayerWPF.Interfaces;
using NAudio.Wave;
using System;
using System.Collections.Generic;

public class AudioService : IAudioService, IDisposable
{
    private WaveOutEvent _waveOut;
    private AudioFileReader? _audioFileReader;
    private EqualizerProvider? _equalizer;
    private List<string> _playlist;
    private int _currentTrackIndex = 0;
    private bool _isRandom = false;
    private bool _isDisposed = false;
    private Random _random;

    public event Action<int>? TrackChanged;

    public AudioService()
    {
        _playlist = new List<string>();
        _waveOut = new WaveOutEvent();
        _waveOut.PlaybackStopped += OnPlaybackStopped;
        _random = new Random();
    }

    public void LoadPlaylist(List<string> playlist)
    {
        if (playlist == null || playlist.Count == 0) throw new ArgumentNullException(nameof(playlist));

        _playlist = playlist;
        LoadTrack(0); // Cargar la primera canción automáticamente
    }

    public void LoadTrack(int index)
    {
        if (index < 0 || index >= _playlist.Count) return;

        Stop();
        _currentTrackIndex = index;
        DisposeAudio();

        try
        {
            _audioFileReader = new AudioFileReader(_playlist[index]);
            _equalizer = new EqualizerProvider(_audioFileReader);
            _waveOut.Init(_equalizer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar el archivo de audio: {ex.Message}");
            return;
        }

        TrackChanged?.Invoke(_currentTrackIndex);
    }

    public void Play()
    {
        if (_audioFileReader == null) return;
        _waveOut.Play();
    }

    public void Pause()
    {
        if (_audioFileReader == null) return;
        _waveOut.Pause();
    }

    public void Stop()
    {
        _waveOut.Stop();
    }

    public void Next()
    {
        if (_playlist.Count == 0) return;

        _currentTrackIndex = _isRandom
            ? _random.Next(0, _playlist.Count)
            : (_currentTrackIndex + 1) % _playlist.Count;

        LoadTrack(_currentTrackIndex);
        Play();
    }

    public void Previous()
    {
        if (_playlist.Count == 0) return;

        _currentTrackIndex = (_currentTrackIndex - 1 + _playlist.Count) % _playlist.Count;

        LoadTrack(_currentTrackIndex);
        Play();
    }

    public void Seek(int seconds)
    {
        if (_audioFileReader == null) return;

        var newTime = _audioFileReader.CurrentTime.Add(TimeSpan.FromSeconds(seconds));
        _audioFileReader.CurrentTime = newTime < TimeSpan.Zero ? TimeSpan.Zero : newTime;
    }

    public void SetRandom(bool isRandom)
    {
        _isRandom = isRandom;
    }

    public void SetEqualizer(float[] bands)
    {
        if (_equalizer == null) return;
        _equalizer.SetBands(bands);
    }

    public void SetEqualizer(List<int> bands)
    {
        SetEqualizer(bands.Select(x => (float)x).ToArray()); // Convertimos List<int> a float[]
    }

    public double GetCurrentPosition()
    {
        return _audioFileReader?.CurrentTime.TotalSeconds ?? 0;
    }

    public double GetTotalTime()
    {
        return _audioFileReader?.TotalTime.TotalSeconds ?? 0;
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            Console.WriteLine($"Error en la reproducción: {e.Exception.Message}");
            return;
        }

        Next();
    }

    private void DisposeAudio()
    {
        _waveOut.Stop();
        _audioFileReader?.Dispose();
        _audioFileReader = null;
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _waveOut.PlaybackStopped -= OnPlaybackStopped;
        _waveOut.Dispose();
        _audioFileReader?.Dispose();
    }
}


