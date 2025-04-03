using UnityEngine;
using UnityEngine.Audio;

public class PlayerFrequency : MonoBehaviour
{
    public float currentFrequency; // The player's current frequency
    public bool isBluePlayer; // Determines if this player is the blue player
    public bool isUsingFrequency = false; // Indicates whether the player is actively using their frequency ability

    [Header("Frequency Settings")]
    public float frequencyRangeMin = 0f; // Minimum frequency the player can set
    public float frequencyRangeMax = 1000f; // Maximum frequency the player can set
    public float frequencyStep = 10f; // Step size for frequency adjustments

    [Header("Audio Settings")]
    public AudioSource audioSource; // Reference to the AudioSource that plays the sound
    public float basePitch = 1f; // The default pitch of the audio
    public float pitchRangeMin = 0.5f; // Minimum pitch value
    public float pitchRangeMax = 2f; // Maximum pitch value
    public float pitchSmoothTime = 0.1f; // Smoothing time for gradual pitch transitions

    private float lastFrequency = -1f; // Stores the last frequency value to detect changes
    private float currentPitchVelocity; // Used for smooth pitch changes

    private SineWaveGenerator sineGenerator; // Generates a sine wave based on frequency
    public AudioMixerGroup playerMixerGroup; // Assign this to route audio to the "Players" mixer group

    void Start()
    {
        // Try to get the existing SineWaveGenerator component or add one if missing
        sineGenerator = GetComponent<SineWaveGenerator>();
        if (sineGenerator == null)
        {
            sineGenerator = gameObject.AddComponent<SineWaveGenerator>();
        }

        // Configure initial settings for the sine wave
        sineGenerator.amplitude = 0.1f; // Set the volume of the sine wave

        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Toggle the frequency ability ON/OFF when pressing 'F'
        if (Input.GetKeyDown(KeyCode.F))
        {
            isUsingFrequency = !isUsingFrequency; // Toggle the boolean value

            if (isUsingFrequency)
            {
                // Ensure the correct audio mixer group is assigned before playing
                if (audioSource.outputAudioMixerGroup == null)
                {
                    audioSource.outputAudioMixerGroup = playerMixerGroup;
                }
                audioSource.Play(); // Start playing the sound
            }
            else
            {
                audioSource.Stop(); // Stop the sound when frequency ability is disabled
            }
        }

        // Adjust frequency only if the player is actively using it
        if (isUsingFrequency)
        {
            HandleFrequencyInput(); // Update frequency based on player input
            currentFrequency = Mathf.Clamp(currentFrequency, frequencyRangeMin, frequencyRangeMax); // Keep frequency within limits

            // Update pitch only if the frequency has changed
            if (Mathf.Abs(currentFrequency - lastFrequency) > 0.1f)
            {
                UpdatePitch(); // Adjust pitch based on frequency
                lastFrequency = currentFrequency; // Store the last frequency value
            }
        }

        // Update the sine wave frequency when the ability is active
        if (isUsingFrequency && sineGenerator != null)
        {
            sineGenerator.frequency = currentFrequency;
        }
    }

    // Handles player input to increase or decrease frequency
    private void HandleFrequencyInput()
    {
        if (isBluePlayer)
        {
            // If the player is blue, they use 1/2 keys to adjust frequency
            if (Input.GetKey(KeyCode.Alpha1))
            {
                currentFrequency += frequencyStep * Time.deltaTime * frequencyStep; // Increase frequency
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                currentFrequency -= frequencyStep * Time.deltaTime * frequencyStep; // Decrease frequency
            }
        }
        else
        {
            // If the player is red, they use Numpad 8 and 2 to adjust frequency
            if (Input.GetKey(KeyCode.Keypad8))
            {
                currentFrequency += frequencyStep * Time.deltaTime * frequencyStep; // Increase frequency
            }
            else if (Input.GetKey(KeyCode.Keypad2))
            {
                currentFrequency -= frequencyStep * Time.deltaTime * frequencyStep; // Decrease frequency
            }
        }
    }

    // Updates the audio pitch based on the current frequency
    private void UpdatePitch()
    {
        if (audioSource == null) return; // Ensure there's an AudioSource before proceeding

        // Normalize the frequency value to a 0-1 range for interpolation
        float normalizedFrequency = Mathf.InverseLerp(frequencyRangeMin, frequencyRangeMax, currentFrequency);

        // Interpolate between min and max pitch based on normalized frequency
        float targetPitch = Mathf.Lerp(pitchRangeMin, pitchRangeMax, normalizedFrequency);

        // Apply smooth pitch transition to prevent sudden changes
        audioSource.pitch = Mathf.SmoothDamp(audioSource.pitch, targetPitch, ref currentPitchVelocity, pitchSmoothTime);
    }
}
