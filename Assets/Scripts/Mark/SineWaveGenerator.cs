using UnityEngine;

[RequireComponent(typeof(AudioSource))] // Ensures an AudioSource is always attached
public class SineWaveGenerator : MonoBehaviour
{
    public float frequency = 440f; // Default frequency (A4 note)
    public float amplitude = 0.1f; // Volume (0 to 1)

    private float sampleRate; // Stores the audio sample rate
    private float phase = 0f; // Keeps track of the waveform phase

    void Start()
    {
        // Get the audio output sample rate
        sampleRate = AudioSettings.outputSampleRate;

        // Configure the AudioSource component
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // Prevent it from playing on start
        audioSource.loop = true; // Ensure the sound loops continuously
    }

    // This function generates the sine wave audio in real-time
    void OnAudioFilterRead(float[] data, int channels)
    {
        // Calculate how much the phase should increment per sample
        float increment = frequency * 2f * Mathf.PI / sampleRate;

        // Loop through each audio sample
        for (int i = 0; i < data.Length; i += channels)
        {
            // Generate the sine wave value
            phase += increment;
            if (phase > 2f * Mathf.PI) phase -= 2f * Mathf.PI; // Keep phase within 0 to 2π

            float value = Mathf.Sin(phase) * amplitude; // Generate the sine wave

            // Apply the sine wave to all audio channels (stereo/mono)
            for (int c = 0; c < channels; c++)
            {
                data[i + c] = value;
            }
        }
    }
}

