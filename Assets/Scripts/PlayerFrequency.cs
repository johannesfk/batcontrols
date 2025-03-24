using UnityEngine;

public class PlayerFrequency : MonoBehaviour
{
    public float currentFrequency;
    public bool isBluePlayer;
    public bool isUsingFrequency = false;  // Tracks if the ability is active

    // Shared frequency range for both players
    public static float frequencyRangeMin = 100f;
    public static float frequencyRangeMax = 1000f;

    void Update()
    {
        // Toggle the frequency ability ON/OFF when pressing F
        if (Input.GetKeyDown(KeyCode.F))
        {
            isUsingFrequency = !isUsingFrequency;  // Toggle state
        }

        // Update frequency if the ability is active
        if (isUsingFrequency)
        {
            // Adjust frequency within the shared range
            currentFrequency = Mathf.Lerp(frequencyRangeMin, frequencyRangeMax, Input.GetAxis("Vertical"));

            // Clamp the frequency to ensure it stays within the valid range
            currentFrequency = Mathf.Clamp(currentFrequency, frequencyRangeMin, frequencyRangeMax);
        }
    }
}

