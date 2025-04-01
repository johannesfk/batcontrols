using UnityEngine;
using UnityEngine.Audio;

public class PlayerFrequency : MonoBehaviour
{
    public float currentFrequency;
    public bool isBluePlayer;
    public bool isUsingFrequency = false;

    [Header("Frequency Settings")]
    public float frequencyRangeMin = 0f;
    public float frequencyRangeMax = 1000f;
    public float frequencyStep = 10f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public float basePitch = 1f;
    public float pitchRangeMin = 0.5f;
    public float pitchRangeMax = 2f;
    public float pitchSmoothTime = 0.1f; // Smoothing time for pitch transitions

    private float lastFrequency = -1f;
    private float currentPitchVelocity; // Used for SmoothDamp

    private SineWaveGenerator sineGenerator;
    public AudioMixerGroup playerMixerGroup; // Assign the "Players" group

    void Start()
    {
        // Get or add the sine wave generator
        sineGenerator = GetComponent<SineWaveGenerator>();
        if (sineGenerator == null)
        {
            sineGenerator = gameObject.AddComponent<SineWaveGenerator>();
        }

        // Configure initial settings
        sineGenerator.amplitude = 0.1f; // Set your desired volume
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Toggle the frequency ability ON/OFF when pressing F
        if (Input.GetKeyDown(KeyCode.F))
        {
            isUsingFrequency = !isUsingFrequency;

            if (isUsingFrequency)
            {
                if (audioSource.outputAudioMixerGroup == null)
                {
                    audioSource.outputAudioMixerGroup = playerMixerGroup;
                }
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }

        // Adjust frequency if the ability is active
        if (isUsingFrequency)
        {
            HandleFrequencyInput();
            currentFrequency = Mathf.Clamp(currentFrequency, frequencyRangeMin, frequencyRangeMax);

            // Update pitch if the frequency changes
            if (Mathf.Abs(currentFrequency - lastFrequency) > 0.1f)
            {
                UpdatePitch();
                lastFrequency = currentFrequency;
            }
        }
        // Update the sine wave frequency when active
        if (isUsingFrequency && sineGenerator != null)
        {
            sineGenerator.frequency = currentFrequency;
        }

    }

    private void HandleFrequencyInput()
    {
        if (isBluePlayer)
        {
            if (Input.GetKey(KeyCode.W))
            {
                currentFrequency += frequencyStep * Time.deltaTime * frequencyStep;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                currentFrequency -= frequencyStep * Time.deltaTime * frequencyStep;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Keypad8))
            {
                currentFrequency += frequencyStep * Time.deltaTime * frequencyStep;
            }
            else if (Input.GetKey(KeyCode.Keypad2))
            {
                currentFrequency -= frequencyStep * Time.deltaTime * frequencyStep;
            }
        }
    }

    private void UpdatePitch()
    {
        if (audioSource == null) return;

        float normalizedFrequency = Mathf.InverseLerp(frequencyRangeMin, frequencyRangeMax, currentFrequency);
        float targetPitch = Mathf.Lerp(pitchRangeMin, pitchRangeMax, normalizedFrequency);

        // Apply smooth damping to the pitch change
        audioSource.pitch = Mathf.SmoothDamp(audioSource.pitch,targetPitch,ref currentPitchVelocity,pitchSmoothTime);
    }
}

