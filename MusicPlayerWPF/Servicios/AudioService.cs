using MusicPlayerWPF.Interfaces;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AudioService : IAudioService, IDisposable
{
    private static readonly Lazy<AudioService> _instance = new Lazy<AudioService>(() => new AudioService());
    public static AudioService Instance => _instance.Value;

    private WaveOutEvent _waveOut;
    private AudioFileReader? _audioFileReader;
    private EqualizerProvider? _equalizer;
    private List<string> _playlist;
    private int _currentTrackIndex = 0;
    private bool _isRandom = false;
    private bool _isDisposed = false;
    private Random _random;
    private bool _isPlaying = false;

    public event Action<int>? TrackChanged;
    public event Action<bool>? PlaybackStateChanged;
    public event Action<string>? ErrorOccurred;

    private AudioService()
    {
        _playlist = new List<string>();
        _waveOut = new WaveOutEvent();
        _waveOut.PlaybackStopped += OnPlaybackStopped;
        _random = new Random();
    }

    public bool IsPlaying()
    {
        return _isPlaying;
    }

    public List<string> GetPlaylist() => _playlist;

    public void LoadPlaylist(List<string> playlist)
    {
        if (playlist == null || playlist.Count == 0)
        {
            ErrorOccurred?.Invoke("La lista de reproducción no puede estar vacía.");
            return;
        }

        var nuevosElementos = playlist.Where(song => !_playlist.Contains(song)).ToList();
        _playlist.AddRange(nuevosElementos);
    }

    public async Task LoadTrackAsync(string cancion)
    {
        if (string.IsNullOrWhiteSpace(cancion))
        {
            ErrorOccurred?.Invoke("La canción no puede estar vacía.");
            return;
        }

        int index = _playlist.IndexOf(cancion);
        if (index == -1)
        {
            ErrorOccurred?.Invoke("La canción no está en la lista de reproducción.");
            return;
        }

        _currentTrackIndex = index;
        DisposeAudio();

        try
        {
            _audioFileReader = new AudioFileReader(_playlist[index]);
            _equalizer = new EqualizerProvider(_audioFileReader);
            _waveOut = new WaveOutEvent();
            _waveOut.PlaybackStopped += OnPlaybackStopped;
            _waveOut.Init(_equalizer);
            TrackChanged?.Invoke(_currentTrackIndex);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke($"Error al cargar la canción: {ex.Message}");
        }
    }

    public void Play()
    {
        if (_audioFileReader == null && _playlist.Count > 0)
        {
            LoadTrackAsync(_playlist[0]).Wait();
        }

        if (_audioFileReader != null)
        {
            _isPlaying = true;
            _waveOut.Play();
            PlaybackStateChanged?.Invoke(true);
        }
    }

    public void Play(string cancion)
    {
        if (string.IsNullOrWhiteSpace(cancion))
        {
            ErrorOccurred?.Invoke("La canción no puede estar vacía.");
            return;
        }

        if (_audioFileReader != null && _playlist[_currentTrackIndex] == cancion)
        {
            if (_waveOut.PlaybackState != PlaybackState.Playing)
            {
                _waveOut.Play();
                PlaybackStateChanged?.Invoke(true);
            }
            return;
        }

        Stop();
        DisposeAudio();

        LoadTrackAsync(cancion).Wait();

        if (_audioFileReader != null)
        {
            _isPlaying = true;
            _waveOut.Play();
            PlaybackStateChanged?.Invoke(true);
        }
    }

    public void Pause()
    {
        if (_audioFileReader == null) return;

        _isPlaying = false;
        _waveOut.Pause();
        PlaybackStateChanged?.Invoke(false);
    }

    public void Stop()
    {
        _isPlaying = false;
        _waveOut.Stop();
        PlaybackStateChanged?.Invoke(false);
    }

    public void Next()
    {
        if (_playlist.Count == 0) return;

        _currentTrackIndex = _isRandom
            ? _random.Next(0, _playlist.Count)
            : (_currentTrackIndex + 1) % _playlist.Count;

        LoadTrackAsync(_playlist[_currentTrackIndex]).Wait();
        Play();
    }

    public void Previous()
    {
        if (_playlist.Count == 0) return;

        _currentTrackIndex = (_currentTrackIndex - 1 + _playlist.Count) % _playlist.Count;

        LoadTrackAsync(_playlist[_currentTrackIndex]).Wait();
        Play();
    }

    public void Seek(int seconds)
    {
        if (_audioFileReader == null) return;

        var newTime = _audioFileReader.CurrentTime.Add(TimeSpan.FromSeconds(seconds));
        _audioFileReader.CurrentTime = newTime < TimeSpan.Zero ? TimeSpan.Zero : newTime;
    }

    public void SetRandom(bool isRandom) => _isRandom = isRandom;

    public void SetEqualizer(float[] bands)
    {
        if (_equalizer == null) return;
        _equalizer.SetBands(bands);
    }

    public void SetEqualizer(List<int> bands) => SetEqualizer(bands.Select(x => (float)x).ToArray());

    public double GetCurrentPosition() => _audioFileReader?.CurrentTime.TotalSeconds ?? 0;

    public double GetTotalTime() => _audioFileReader?.TotalTime.TotalSeconds ?? 0;

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            ErrorOccurred?.Invoke($"Error en la reproducción: {e.Exception.Message}");
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