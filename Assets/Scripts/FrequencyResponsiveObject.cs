using UnityEngine;

public class FrequencyResponsiveObject : MonoBehaviour
{
    public float requiredFrequency;  // The target frequency for interaction
    public float activationRange = 5f;  // How close the player needs to be
    public Transform bluePlayer;
    public Transform redPlayer;

    public Vector3 minScale = new Vector3(0.5f, 0.5f, 0.5f);  // Minimum size
    public Vector3 maxScale = new Vector3(3f, 3f, 3f);  // Maximum size
    public float growthSpeed = 0.5f;  // Speed of growing/shrinking

    private bool isGrowing = false;
    private bool isShrinking = false;

    private float lastBlueFrequency = -1f; // Store last logged frequency for Blue
    private float lastRedFrequency = -1f;  // Store last logged frequency for Red

    void Update()
    {
        // Get frequency components for both blue and red players
        PlayerFrequency blueFreq = bluePlayer.GetComponent<PlayerFrequency>();
        PlayerFrequency redFreq = redPlayer.GetComponent<PlayerFrequency>();

        if (blueFreq == null || redFreq == null) return; // Prevent errors if references are missing

        // Get the shared frequency range from an instance of PlayerFrequency
        float frequencyRangeMin = blueFreq.frequencyRangeMin;
        float frequencyRangeMax = blueFreq.frequencyRangeMax;

        // Sync frequencies for both players within the same range
        float blueFrequency = Mathf.Clamp(blueFreq.currentFrequency, frequencyRangeMin, frequencyRangeMax);
        float redFrequency = Mathf.Clamp(redFreq.currentFrequency, frequencyRangeMin, frequencyRangeMax);

        // Log only when the frequency changes
        if (blueFreq.isUsingFrequency && Mathf.Abs(blueFrequency - lastBlueFrequency) > 0.1f)
        {
            Debug.Log($"Blue Player using frequency: {blueFrequency}");
            lastBlueFrequency = blueFrequency;
        }

        if (redFreq.isUsingFrequency && Mathf.Abs(redFrequency - lastRedFrequency) > 0.1f)
        {
            Debug.Log($"Red Player using frequency: {redFrequency}");
            lastRedFrequency = redFrequency;
        }

        // Determine if the players can interact with the object
        bool blueCanGrow = blueFreq.isUsingFrequency && IsPlayerInRange(bluePlayer) && Mathf.Abs(blueFrequency - requiredFrequency) < 10f;
        bool redCanShrink = redFreq.isUsingFrequency && IsPlayerInRange(redPlayer) && Mathf.Abs(redFrequency - requiredFrequency) < 10f;

        // If both players try to interact at the same time, cancel the effect
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

        HandleGrowth();
    }

    bool IsPlayerInRange(Transform player)
    {
        return Vector3.Distance(player.position, transform.position) <= activationRange;
    }

    void HandleGrowth()
    {
        Vector3 targetScale = transform.localScale;

        if (isGrowing && transform.localScale.x < maxScale.x)
        {
            targetScale = Vector3.MoveTowards(transform.localScale, maxScale, growthSpeed * Time.deltaTime);
        }
        else if (isShrinking && transform.localScale.x > minScale.x)
        {
            targetScale = Vector3.MoveTowards(transform.localScale, minScale, growthSpeed * Time.deltaTime);
        }

        // Calculate growth difference
        float scaleDifference = targetScale.y - transform.localScale.y;

        // Apply new scale
        transform.localScale = targetScale;

        // Adjust position to keep the bottom on the ground
        transform.position += new Vector3(0, scaleDifference / 2f, 0);
    }
}
