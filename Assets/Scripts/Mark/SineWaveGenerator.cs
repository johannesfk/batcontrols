using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SineWaveGenerator : MonoBehaviour
{
    public float frequency = 440f; // Default frequency (A4 note)
    public float amplitude = 0.1f; // Volume (0 to 1)

    private float sampleRate;
    private float phase = 0f;

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        GetComponent<AudioSource>().playOnAwake = false;
        GetComponent<AudioSource>().loop = true;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        float increment = frequency * 2f * Mathf.PI / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += increment;
            if (phase > 2f * Mathf.PI) phase -= 2f * Mathf.PI;

            float value = Mathf.Sin(phase) * amplitude;

            // Apply to all channels (mono or stereo)
            for (int c = 0; c < channels; c++)
            {
                data[i + c] = value;
            }
        }
    }
}
