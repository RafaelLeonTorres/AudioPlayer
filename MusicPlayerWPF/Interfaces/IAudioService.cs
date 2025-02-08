using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerWPF.Interfaces
{
    public interface IAudioService
    {
        void LoadPlaylist(List<string> playlist);
        void LoadTrack(int index);
        void Play();
        void Pause();
        void Stop();
        void Next();
        void Previous();
        void Seek(int seconds);
        void SetRandom(bool isRandom);
        void SetEqualizer(float[] bands);
        double GetCurrentPosition();  // Nuevo método para obtener la posición actual de la canción
        double GetTotalTime();        // Duración total de la canción
    }
}
