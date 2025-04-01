using UnityEngine;

public class FrequencyVisualizer : MonoBehaviour
{
    public PlayerFrequency playerFrequency;  // Reference to the PlayerFrequency script
    public RectTransform colorBar;          // The UI image representing the frequency range
    public float minY;                      // Bottom position of the arrow (min frequency)
    public float maxY;                      // Top position of the arrow (max frequency)

    void Update()
    {
        if (playerFrequency != null && colorBar != null)
        {
            // Normalize frequency value between 0 and 1 using the player's instance variables
            float normalizedFrequency = Mathf.InverseLerp(playerFrequency.frequencyRangeMin,
                                                          playerFrequency.frequencyRangeMax,
                                                          playerFrequency.currentFrequency);

            // Calculate new Y position
            float newY = Mathf.Lerp(minY, maxY, normalizedFrequency);

            // Move the polygon (UI element)
            GetComponent<RectTransform>().anchoredPosition = new Vector2(
                GetComponent<RectTransform>().anchoredPosition.x, newY);
        }
    }
}
