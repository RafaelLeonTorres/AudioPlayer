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
        private List<string> _playlist;
        private float[] _equalizerGains = new float[10]; // Almacena los valores de las 10 bandas

        private DispatcherTimer _progressTimer; // Temporizador para actualizar la barra de progreso

        public MainWindow()
        {
            InitializeComponent();
            _audioService = new AudioService();
            _playlist = new List<string>();
            PlaylistView.ItemsSource = _playlist;

            // Inicializa el temporizador
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000) // Actualiza cada segundo
            };
            _progressTimer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Obtener el tiempo de reproducción actual y la duración total de la canción
            var currentTime = _audioService.GetCurrentPosition();  // Tiempo actual en segundos
            var totalTime = _audioService.GetTotalTime();          // Duración total de la canción en segundos

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
                bool isFirstAdd = _playlist.Count == 0;

                // Añadir canciones a la lista, evitando duplicados
                foreach (string filePath in openFileDialog.FileNames)
                {
                    if (!_playlist.Contains(filePath))
                    {
                        _playlist.Add(filePath);
                    }
                }

                PlaylistView.Items.Refresh();

                // Solo carga y reproduce si es la primera vez que se agrega canciones
                if (isFirstAdd && _playlist.Count > 0)
                {
                    _audioService.LoadPlaylist(_playlist);
                    _audioService.Play();
                }
            }
        }

        private void PlaylistView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Verificar si un ítem ha sido seleccionado
            var selectedItem = PlaylistView.SelectedItem as string; // Asumiendo que la lista contiene rutas de archivo como strings

            if (selectedItem != null)
            {
                _audioService.Stop();
                // Buscar el índice del archivo seleccionado directamente en la lista
                int selectedIndex = _playlist.IndexOf(selectedItem);

                if (selectedIndex >= 0)
                {
                    // Cargar y reproducir la canción seleccionada
                    _audioService.LoadTrack(selectedIndex);
                    _audioService.Play();
                }
            }
        }


        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Ajustar la posición de la canción al mover la barra de progreso
            var position = _audioService.GetCurrentPosition();
            ProgressBar.Value = position;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            _audioService.Play();
            _progressTimer.Start(); // Inicia el temporizador para actualizar la barra de progreso
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _audioService.Pause();
            _progressTimer.Stop(); // Detiene el temporizador cuando se pausa
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            _audioService.Next();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            _audioService.Previous();
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            _audioService.SetRandom(true);
        }

        private void SeekBack_Click(object sender, RoutedEventArgs e)
        {
            _audioService.Seek(-10);
        }

        private void SeekForward_Click(object sender, RoutedEventArgs e)
        {
            _audioService.Seek(10);
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

                    // Aplicar los cambios al servicio de audio
                    try
                    {
                        _audioService.SetEqualizer(_equalizerGains);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al aplicar el ecualizador: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Índice de banda fuera de rango.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
