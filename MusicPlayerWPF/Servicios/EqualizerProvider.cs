using NAudio.Dsp;
using NAudio.Wave;
using System;

public class EqualizerProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly BiQuadFilter[] _bands;
    private readonly float[] _frequencies; // Guardamos las frecuencias

    public EqualizerProvider(ISampleProvider source)
    {
        _source = source;
        _bands = new BiQuadFilter[10];

        // Frecuencias comunes para un ecualizador de 10 bandas
        _frequencies = new float[] { 32, 64, 125, 250, 500, 1000, 2000, 4000, 8000, 16000 };

        for (int i = 0; i < _bands.Length; i++)
        {
            _bands[i] = BiQuadFilter.PeakingEQ(44100, _frequencies[i], 1.0f, 0);
        }
    }

    public void SetBands(float[] gains)
    {
        if (gains.Length != _bands.Length) return;

        for (int i = 0; i < _bands.Length; i++)
        {
            _bands[i] = BiQuadFilter.PeakingEQ(44100, _frequencies[i], 1.0f, gains[i]);
        }
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        // Validación de parámetros de entrada
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer), "El buffer no puede ser nulo.");
        }

        if (offset < 0 || offset >= buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "El valor de offset es inválido.");
        }

        if (count <= 0 || (offset + count) > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "El valor de count es inválido o excede el tamaño del buffer.");
        }

        int samplesRead = _source.Read(buffer, offset, count);

        // Asegúrate de que samplesRead no exceda el rango de los índices
        if (samplesRead <= 0)
        {
            return 0;  // No se leyeron muestras, por lo que no hay nada que procesar
        }

        // Procesa las muestras y aplica la transformación de las bandas
        for (int i = 0; i < samplesRead; i++)
        {
            float sample = buffer[offset + i];

            // Aplica la transformación a cada banda
            foreach (var band in _bands)
            {
                try
                {
                    sample = band.Transform(sample);
                }
                catch (Exception ex)
                {
                    // Maneja cualquier error dentro de la transformación de banda
                    Console.WriteLine($"Error al aplicar la transformación de banda: {ex.Message}");
                    // Puedes decidir cómo manejar este error: por ejemplo, aplicar un valor por defecto o continuar
                }
            }

            buffer[offset + i] = sample;
        }

        return samplesRead;
    }
}
