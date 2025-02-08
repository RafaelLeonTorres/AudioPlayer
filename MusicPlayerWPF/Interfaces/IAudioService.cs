using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerWPF.Interfaces
{
    public interface IAudioService
    {
        List<string> GetPlaylist();
        void LoadPlaylist(List<string> playlist);
        Task LoadTrackAsync(string cancion);
        void Play();
        void Play(string cancion);
        void Pause();
        void Stop();
        void Next();
        void Previous();
        void Seek(int seconds);
        void SetRandom(bool isRandom);
        void SetEqualizer(float[] bands);
        void SetEqualizer(List<int> bands);
        double GetCurrentPosition();
        double GetTotalTime();
        bool IsPlaying(); // Nuevo método para verificar si se está reproduciendo
        void Dispose();
        event Action<int> TrackChanged;
        event Action<bool> PlaybackStateChanged;
        event Action<string> ErrorOccurred;
    }
}
