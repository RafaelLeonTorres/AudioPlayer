using MusicPlayerWPF.Interfaces;

namespace MusicPlayerWPF.Logica
{
    public class PlayTrackUseCase
    {
        private readonly IAudioService _audioService;

        public PlayTrackUseCase(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public void Execute(int trackIndex)
        {
            _audioService.LoadTrack(trackIndex);
            _audioService.Play();
        }
    }
}
