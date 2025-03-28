using UnityEngine;

public class PlayerFrequency : MonoBehaviour
{
    public float currentFrequency;
    public bool isBluePlayer;
    public bool isUsingFrequency = false;  // Tracks if the ability is active

    public float frequencyRangeMin = 0f;
    public float frequencyRangeMax = 1000f;
    public float frequencyStep = 10f; // Step size for increasing/decreasing frequency

    void Update()
    {
        // Toggle the frequency ability ON/OFF when pressing F
        if (Input.GetKeyDown(KeyCode.F))
        {
            isUsingFrequency = !isUsingFrequency;  // Toggle state
        }

        // Adjust frequency if the ability is active
        if (isUsingFrequency)
        {
            if (isBluePlayer)
            {
                // Increase/decrease frequency with W/S
                if (Input.GetKey(KeyCode.W))
                {
                    currentFrequency += frequencyStep * Time.deltaTime * 10;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    currentFrequency -= frequencyStep * Time.deltaTime * 10;
                }
            }
            else
            {
                // Increase/decrease frequency with keypad 8/2  
                if (Input.GetKey(KeyCode.Keypad8))
                {
                    currentFrequency += frequencyStep * Time.deltaTime * 10;
                }
                else if (Input.GetKey(KeyCode.Keypad2))
                {
                    currentFrequency -= frequencyStep * Time.deltaTime * 10;
                }
            }

            // Clamp frequency within allowed range
            currentFrequency = Mathf.Clamp(currentFrequency, frequencyRangeMin, frequencyRangeMax);
        }
    }
}


