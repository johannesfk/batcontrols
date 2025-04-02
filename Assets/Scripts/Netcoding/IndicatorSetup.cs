using UnityEngine;
using UnityEngine.UI;

public class IndicatorSetup : MonoBehaviour
{
    public Color indicatorColor = Color.red;
    public float indicatorSize = 50f;
    
    private void Awake()
    {
        // Get or add the required components
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        // Set size
        rectTransform.sizeDelta = new Vector2(indicatorSize, indicatorSize);
        
        // Add an image component if not present
        Image image = GetComponent<Image>();
        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
        }
        
        // Configure the image
        image.color = indicatorColor;
        
        // You could use a custom sprite here
        // image.sprite = yourCustomSprite;
    }
}