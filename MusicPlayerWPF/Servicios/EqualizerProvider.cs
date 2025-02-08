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
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "El proveedor de muestras no puede ser nulo.");
        }

        _source = source;
        _bands = new BiQuadFilter[10];

        // Frecuencias comunes para un ecualizador de 10 bandas
        _frequencies = new float[] { 32, 64, 125, 250, 500, 1000, 2000, 4000, 8000, 16000 };

        // Inicializa los filtros para cada banda
        for (int i = 0; i < _bands.Length; i++)
        {
            _bands[i] = BiQuadFilter.PeakingEQ(_source.WaveFormat.SampleRate, _frequencies[i], 1.0f, 0);
        }
    }

    public void SetBands(float[] gains)
    {
        if (gains == null)
        {
            throw new ArgumentNullException(nameof(gains), "El arreglo de ganancias no puede ser nulo.");
        }

        if (gains.Length != _bands.Length)
        {
            throw new ArgumentException("El número de ganancias debe coincidir con el número de bandas.", nameof(gains));
        }

        // Actualiza los filtros con las nuevas ganancias
        for (int i = 0; i < _bands.Length; i++)
        {
            _bands[i] = BiQuadFilter.PeakingEQ(_source.WaveFormat.SampleRate, _frequencies[i], 1.0f, gains[i]);
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

        // Lee las muestras del proveedor de origen
        int samplesRead = _source.Read(buffer, offset, count);

        // Si no se leyeron muestras, retornar 0
        if (samplesRead <= 0)
        {
            return 0;
        }

        // Procesa las muestras y aplica la transformación de las bandas
        for (int i = 0; i < samplesRead; i++)
        {
            float sample = buffer[offset + i];

            // Aplica la transformación a cada banda
            foreach (var band in _bands)
            {
                if (band != null) // Asegúrate de que el filtro no sea nulo
                {
                    sample = band.Transform(sample);
                }
            }

            buffer[offset + i] = sample;
        }

        return samplesRead;
    }
}