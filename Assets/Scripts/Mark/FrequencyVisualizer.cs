using UnityEngine;

public class FrequencyVisualizer : MonoBehaviour
{
    public PlayerFrequency playerFrequency;  // Reference to the PlayerFrequency script
    public RectTransform colorBar;          // UI bar that represents frequency range
    public RectTransform arrow;             // UI arrow indicating frequency
    public float minY;                      // Bottom position of the arrow (min frequency)
    public float maxY;                      // Top position of the arrow (max frequency)

    void Update()
    {
        if (playerFrequency == null || colorBar == null || arrow == null) return;

        // Check if frequency is active and show/hide UI elements accordingly
        bool isActive = playerFrequency.isUsingFrequency;
        colorBar.gameObject.SetActive(isActive);
        arrow.gameObject.SetActive(isActive);

        if (isActive)
        {
            // Normalize frequency value between 0 and 1
            float normalizedFrequency = Mathf.InverseLerp(
                playerFrequency.frequencyRangeMin,
                playerFrequency.frequencyRangeMax,
                playerFrequency.currentFrequency
            );

            // Move the arrow based on frequency
            float newY = Mathf.Lerp(minY, maxY, normalizedFrequency);
            arrow.localPosition = new Vector3(arrow.localPosition.x, newY, arrow.localPosition.z);
        }
    }
}



