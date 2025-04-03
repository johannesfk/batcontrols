using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

// Ensures the GameObject has an AudioSource component
[RequireComponent(typeof(AudioSource))]
public class FrequencyResponsiveObject : MonoBehaviour
{
    // ===== FREQUENCY SETTINGS =====
    [Header("Frequency Settings")]
    public float requiredFrequency = 440f;  // Default frequency (A4 note)
    public float frequencyTolerance = 10f;  // Acceptable range around requiredFrequency
    public float activationRange = 5f;      // Range in which the player affects the object

    // ===== VISUAL SETTINGS =====
    [Header("Visual Settings")]
    public Vector3 minScale = new Vector3(0.5f, 0.5f, 0.5f);  // Minimum object scale
    public Vector3 maxScale = new Vector3(3f, 3f, 3f);        // Maximum object scale
    public float growthSpeed = 0.5f;                          // Speed of size changes

    public Transform bluePlayer;  // Reference to the blue player
    public Transform redPlayer;   // Reference to the red player

    private bool isGrowing = false;   // Flag for growth state
    private bool isShrinking = false; // Flag for shrink state

    // ===== AUDIO SETTINGS =====
    [Header("Audio Settings")]
    public float emissionVolume = 0.3f;       // Base volume when idle
    public float pitchVariation = 0.1f;       // Pitch fluctuation when active
    public float activeVolumeBoost = 1.5f;    // Volume increase when active
    public float smoothTime = 0.2f;           // Smoothing time for volume/pitch transitions
    private AudioSource audioSource;          // Audio source component
    private float basePitch;                  // Base pitch of the audio
    private float currentPitch;               // Current adjusted pitch
    private float targetVolume;               // Desired volume level
    private float volumeVelocity;             // Used for smooth volume changes
    private float pitchVelocity;              // Used for smooth pitch changes

    public AudioMixerGroup environmentMixerGroup; // Assign the "Environment" mixer group

    private float lastBlueFreq;  // Stores the last frequency of the blue player
    private float lastRedFreq;   // Stores the last frequency of the red player

    private Bug bugScript; // Reference to the Bug script
    void Start()
    {
        // Check if this object has a Bug component and store reference
        bugScript = GetComponent<Bug>();

        audioSource = GetComponent<AudioSource>();
        InitializeAudio();  
    }

    // ===== INITIALIZE AUDIO SETTINGS =====
    void InitializeAudio()
    {
        audioSource.outputAudioMixerGroup = environmentMixerGroup; // Assign mixer group
        audioSource.loop = true;    // Loop audio
        audioSource.volume = 0f;    // Start muted
        targetVolume = emissionVolume;
        basePitch = Random.Range(0.95f, 1.05f); // Slight pitch variation for uniqueness
        currentPitch = basePitch;
        audioSource.pitch = currentPitch;

        // Generate a sine wave tone for this object's required frequency
        AudioClip tone = GenerateSineWave(requiredFrequency, 10f); // 10-second clip
        audioSource.clip = tone;
        audioSource.Play();

        // Smoothly fade in the audio
        StartCoroutine(AudioFade(0f, emissionVolume, 2f));
    }

    // ===== FADE AUDIO VOLUME OVER TIME =====
    IEnumerator AudioFade(float startVolume, float endVolume, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = endVolume;
    }

    void Update()
    {
        if (bluePlayer == null || redPlayer == null) return;

        PlayerFrequency blueFreq = bluePlayer.GetComponent<PlayerFrequency>();
        PlayerFrequency redFreq = redPlayer.GetComponent<PlayerFrequency>();

        // Log the players' frequency changes
        if (blueFreq.currentFrequency != lastBlueFreq || redFreq.currentFrequency != lastRedFreq)
        {
            Debug.Log($"<color=blue>Blue</color>: {blueFreq.currentFrequency} Hz | <color=red>Red</color>: {redFreq.currentFrequency} Hz");
            lastBlueFreq = blueFreq.currentFrequency;
            lastRedFreq = redFreq.currentFrequency;
        }

        if (blueFreq == null || redFreq == null) return;

        HandleGrowth();
        UpdateInteractionState(blueFreq, redFreq);
        UpdateAudioFeedback();
    }

    // ===== CHECK IF PLAYERS CAN INTERACT WITH OBJECT =====
    void UpdateInteractionState(PlayerFrequency blueFreq, PlayerFrequency redFreq)
    {
        bool blueCanGrow = blueFreq.isUsingFrequency &&
                           IsPlayerInRange(bluePlayer) &&
                           Mathf.Abs(blueFreq.currentFrequency - requiredFrequency) < frequencyTolerance;

        bool redCanShrink = redFreq.isUsingFrequency &&
                            IsPlayerInRange(redPlayer) &&
                            Mathf.Abs(redFreq.currentFrequency - requiredFrequency) < frequencyTolerance;

        // If both players interact simultaneously, cancel effects
        if (blueCanGrow && redCanShrink)
        {
            isGrowing = false;
            isShrinking = false;
        }
        else if (blueCanGrow)
        {
            isGrowing = true;
            isShrinking = false;
        }
        else if (redCanShrink)
        {
            isGrowing = false;
            isShrinking = true;
        }
        else
        {
            isGrowing = false;
            isShrinking = false;
        }
    }

    // ===== GENERATE A SINE WAVE AUDIO CLIP =====
    AudioClip GenerateSineWave(float frequency, float duration)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float increment = frequency * 2f * Mathf.PI / sampleRate;
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = Mathf.Sin(phase);
            phase += increment;
            if (phase > 2f * Mathf.PI) phase -= 2f * Mathf.PI;
        }

        AudioClip clip = AudioClip.Create("SineWave_" + frequency + "Hz", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // ===== CHECK IF PLAYER IS WITHIN RANGE =====
    bool IsPlayerInRange(Transform player)
    {
        return Vector3.Distance(player.position, transform.position) <= activationRange;
    }

    // ===== HANDLE OBJECT GROWTH/SHRINKING =====
    void HandleGrowth()
    {
        Vector3 targetScale = transform.localScale;

        if (isGrowing)
        {
            targetScale = Vector3.MoveTowards(transform.localScale, maxScale, growthSpeed * Time.deltaTime);
        }
        else if (isShrinking)
        {
            targetScale = Vector3.MoveTowards(transform.localScale, minScale, growthSpeed * Time.deltaTime);
        }

        // Adjust position to keep the object grounded
        if (targetScale != transform.localScale)
        {
            float scaleDifference = targetScale.y - transform.localScale.y;
            transform.localScale = targetScale;
            transform.position += new Vector3(0, scaleDifference / 2f, 0);
        }
        // If this object has a Bug script, update its isBig value
        if (bugScript != null)
        {
            bugScript.isBig = (transform.localScale == maxScale);
        }
    }

    // ===== UPDATE AUDIO BASED ON STATE =====
    void UpdateAudioFeedback()
    {
        // Adjust volume when active
        targetVolume = (isGrowing || isShrinking) ? emissionVolume * activeVolumeBoost : emissionVolume;
        audioSource.volume = Mathf.SmoothDamp(audioSource.volume, targetVolume, ref volumeVelocity, smoothTime);

        // Apply slight pitch variation when active
        float targetPitch = basePitch;
        if (isGrowing || isShrinking)
        {
            targetPitch += Mathf.Sin(Time.time * 2f) * pitchVariation;
        }

        // Smooth pitch transition
        currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, smoothTime);
        audioSource.pitch = currentPitch;
    }

    void OnDestroy()
    {
        // Clean up the generated audio clip
        if (audioSource != null && audioSource.clip != null)
        {
            Destroy(audioSource.clip);
        }
    }
}
