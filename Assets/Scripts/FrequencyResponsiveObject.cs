    using UnityEngine;
    using System.Collections;
    using UnityEngine.Audio;

    [RequireComponent(typeof(AudioSource))]
    public class FrequencyResponsiveObject : MonoBehaviour
    {
        [Header("Frequency Settings")]
        public float requiredFrequency = 440f;  // Default to A4 note
        public float frequencyTolerance = 10f;
        public float activationRange = 5f;

        [Header("Visual Settings")]
        public Vector3 minScale = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 maxScale = new Vector3(3f, 3f, 3f);
        public float growthSpeed = 0.5f;

        public Transform bluePlayer;
        public Transform redPlayer;

        private bool isGrowing = false;
        private bool isShrinking = false;

        [Header("Audio Settings")]
        public float emissionVolume = 0.3f;
        public float pitchVariation = 0.1f;
        public float activeVolumeBoost = 1.5f;
        public float smoothTime = 0.2f;
        private AudioSource audioSource;
        private float basePitch;
        private float currentPitch;
        private float targetVolume;
        private float volumeVelocity;
        private float pitchVelocity;

        public AudioMixerGroup environmentMixerGroup; // Assign the "Environment" group

        private float lastBlueFreq;
        private float lastRedFreq;


    void Start()
        {
            audioSource = GetComponent<AudioSource>();
            InitializeAudio();
        }

        void InitializeAudio()
        {
            audioSource.outputAudioMixerGroup = environmentMixerGroup;
            audioSource.loop = true;
            audioSource.volume = 0f; // Start silent
            targetVolume = emissionVolume;
            basePitch = Random.Range(0.95f, 1.05f);
            currentPitch = basePitch;
            audioSource.pitch = currentPitch;

            // Generate and play the tone
            AudioClip tone = GenerateSineWave(requiredFrequency, 10f); // 10 second clip
            audioSource.clip = tone;
            audioSource.Play();

            // Fade in the audio
            StartCoroutine(AudioFade(0f, emissionVolume, 2f));
        }

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

        void UpdateInteractionState(PlayerFrequency blueFreq, PlayerFrequency redFreq)
        {
            bool blueCanGrow = blueFreq.isUsingFrequency &&
                              IsPlayerInRange(bluePlayer) &&
                              Mathf.Abs(blueFreq.currentFrequency - requiredFrequency) < frequencyTolerance;

            bool redCanShrink = redFreq.isUsingFrequency &&
                               IsPlayerInRange(redPlayer) &&
                               Mathf.Abs(redFreq.currentFrequency - requiredFrequency) < frequencyTolerance;

            // Handle state transitions
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

        bool IsPlayerInRange(Transform player)
        {
            return Vector3.Distance(player.position, transform.position) <= activationRange;
        }

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

            if (targetScale != transform.localScale)
            {
                float scaleDifference = targetScale.y - transform.localScale.y;
                transform.localScale = targetScale;
                transform.position += new Vector3(0, scaleDifference / 2f, 0);
            }
        }

        void UpdateAudioFeedback()
        {
            // Update target volume based on state
            targetVolume = (isGrowing || isShrinking) ? emissionVolume * activeVolumeBoost : emissionVolume;

            // Smooth volume transition
            audioSource.volume = Mathf.SmoothDamp(audioSource.volume, targetVolume, ref volumeVelocity, smoothTime);

            // Calculate target pitch with variation when active
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
            // Clean up the audio clip to prevent memory leaks
            if (audioSource != null && audioSource.clip != null)
            {
                Destroy(audioSource.clip);
            }
        }
    }