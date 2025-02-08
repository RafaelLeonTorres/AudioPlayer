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
        void LoadTrack(string cancion);
        void Play();
        void Pause();
        void Stop();
        void Next();
        void Previous();
        void Seek(int seconds);
        void SetRandom(bool isRandom);
        void SetEqualizer(float[] bands);
        double GetCurrentPosition();
        double GetTotalTime();
        bool IsPlaying();
        void Dispose();
        event Action<int> TrackChanged;
        event Action<bool> PlaybackStateChanged;
        event Action<string> ErrorOccurred;
    }
}
