using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using MusicPlayerWPF.Interfaces;
using MusicPlayerWPF.Utilidades;

namespace MusicPlayerWPF
{
    public partial class MainWindow : Window
    {
        private readonly IAudioService _audioService;
        private float[] _equalizerGains = new float[10]; // Almacena los valores de las 10 bandas
        private DispatcherTimer _progressTimer; // Temporizador para actualizar la barra de progreso

        public MainWindow()
        {
            InitializeComponent();
            _audioService = new AudioService(); // Crear una instancia de AudioService
            PlaylistView.ItemsSource = _audioService.GetPlaylist();

            // Suscribirse a los eventos
            _audioService.TrackChanged += OnTrackChanged;
            _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
            _audioService.ErrorOccurred += OnErrorOccurred;

            // Inicializa el temporizador
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000) // Actualiza cada segundo
            };
            _progressTimer.Tick += Timer_Tick;
        }

        private void OnTrackChanged(int trackIndex)
        {
            // Actualizar la interfaz de usuario cuando cambia la pista
            PlaylistView.SelectedIndex = trackIndex;
            PlaylistView.ScrollIntoView(PlaylistView.SelectedItem);
        }

        private void OnPlaybackStateChanged(bool isPlaying)
        {
            // Actualizar la interfaz de usuario cuando cambia el estado de reproducción
            PlayPauseButton.Content = isPlaying ? "⏸" : "▶";
        }

        private void OnErrorOccurred(string errorMessage)
        {
            // Mostrar mensajes de error
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Obtener el tiempo de reproducción actual y la duración total de la canción
            var currentTime = _audioService.GetCurrentPosition();  // Tiempo actual en segundos
            double totalTime = _audioService.GetTotalTime();       // Duración total de la canción en segundos

            // Actualiza la barra de progreso si hay tiempo total
            if (totalTime > 0)
            {
                ProgressBar.Value = (currentTime / totalTime) * 100; // Convertir a porcentaje
            }
        }

        private void AddSongs_Click(object sender, RoutedEventArgs e)
        {
            // Abre el diálogo de selección de archivos de audio
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Archivos de audio|*.mp3;*.wav;*.aac|Todos los archivos|*.*",
                Title = "Seleccionar canciones"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileNames.Any())
                {
                    List<string> list = openFileDialog.FileNames.ToList();
                    _audioService.LoadPlaylist(list);
                    PlaylistView.ItemsSource = _audioService.GetPlaylist();
                    PlaylistView.Items.Refresh();
                }
            }
        }

        private void PlaylistView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Verificar si un ítem ha sido seleccionado
            var cancion = PlaylistView.SelectedItem as string; // Asumiendo que la lista contiene rutas de archivo como strings

            if (cancion != null)
            {
                // Cargar y reproducir la canción seleccionada
                _audioService.LoadTrack(cancion);
                _audioService.Play();
                _progressTimer.Start(); // Inicia el temporizador para actualizar la barra de progreso
            }
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Ajustar la posición de la canción al mover la barra de progreso
            if (ProgressBar.IsMouseCaptureWithin)
            {
                double newPosition = (ProgressBar.Value / 100) * _audioService.GetTotalTime();
                _audioService.Seek((int)newPosition);
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_audioService.IsPlaying())
            {
                _audioService.Pause();
            }
            else
            {
                _audioService.Play();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            _audioService.Next();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            _audioService.Previous();
        }

        private void EqualizerChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                // El índice de la banda se pasa como Tag
                int bandIndex = (int)slider.Tag;

                if (bandIndex >= 0 && bandIndex < _equalizerGains.Length)
                {
                    float ganancia = (float)slider.Value;

                    // Actualizar las ganancias del ecualizador
                    _equalizerGains[bandIndex] = ganancia;
                    _audioService.SetEqualizer(_equalizerGains);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Liberar recursos al cerrar la ventana
            _audioService.Dispose();
            base.OnClosed(e);
        }
    }
}
