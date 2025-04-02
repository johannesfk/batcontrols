using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ModifiedShowCallRenderer : MonoBehaviour
{
    // Prefab for the GUI indicator
    public RectTransform indicatorPrefab;
    
    // Parent canvas for the indicators
    public Transform indicatorParent;
    
    // Debug text 
    public TMP_Text debugText;
    
    // Dictionary to store player indicators (NetworkObjectId -> PlayerIndicator)
    private Dictionary<ulong, PlayerIndicator> playerIndicators = new Dictionary<ulong, PlayerIndicator>();
    
    // Class to store indicator data for each player
    private class PlayerIndicator
    {
        public Transform playerTransform;
        public RectTransform indicator;
        public Coroutine activeShowCallCoroutine;
        public ulong networkId;
        
        public PlayerIndicator(Transform transform, RectTransform indicator, ulong id)
        {
            playerTransform = transform;
            this.indicator = indicator;
            networkId = id;
            this.indicator.localScale = Vector3.zero; // Start invisible
        }
    }

    void Start()
    {
        if (indicatorPrefab == null)
        {
            Debug.LogError("Indicator prefab is not assigned!");
        }
        
        if (indicatorParent == null)
        {
            // Try to find a canvas in the scene
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                indicatorParent = canvas.transform;
            }
            else
            {
                Debug.LogError("Indicator parent is not assigned and no Canvas was found!");
            }
        }
    }

    void Update()
    {
        UpdateAllIndicatorPositions();
        
        // Debug info
        if (debugText != null)
        {
            string debug = "Player Indicators: " + playerIndicators.Count;
            foreach (var entry in playerIndicators)
            {
                debug += $"\nPlayer {entry.Key}: {(entry.Value.indicator.localScale != Vector3.zero ? "Visible" : "Hidden")}";
            }
            debugText.text = debug;
        }
    }

    // Set up indicators for all other players
    public void SetupPlayerIndicators(List<PlayerShowcaseNetwork> otherPlayers)
    {
        // Clear existing indicators first
        ClearAllIndicators();
        
        // Create new indicators for each player
        foreach (var player in otherPlayers)
        {
            AddPlayerIndicator(player.GetPlayerTransform(), player.GetNetworkObjectId());
        }
    }
    
    // Add a new player indicator
    private void AddPlayerIndicator(Transform playerTransform, ulong networkId)
    {
        // Don't add duplicates
        if (playerIndicators.ContainsKey(networkId))
            return;
            
        if (indicatorPrefab == null || indicatorParent == null)
            return;
            
        // Instantiate the indicator
        RectTransform indicator = Instantiate(indicatorPrefab, indicatorParent);
        indicator.localScale = Vector3.zero; // Start invisible
        
        // Add to dictionary
        playerIndicators.Add(networkId, new PlayerIndicator(playerTransform, indicator, networkId));
    }
    
    // Clear all indicators
    private void ClearAllIndicators()
    {
        foreach (var entry in playerIndicators)
        {
            if (entry.Value.activeShowCallCoroutine != null)
            {
                StopCoroutine(entry.Value.activeShowCallCoroutine);
            }
            
            if (entry.Value.indicator != null)
            {
                Destroy(entry.Value.indicator.gameObject);
            }
        }
        
        playerIndicators.Clear();
    }
    
    // Update positions of all indicators
    private void UpdateAllIndicatorPositions()
    {
        if (Camera.main == null) return;
        
        foreach (var entry in playerIndicators)
        {
            PlayerIndicator playerIndicator = entry.Value;
            
            if (playerIndicator.playerTransform == null || playerIndicator.indicator == null)
                continue;
                
            // Convert world position to screen position
            Vector3 rawScreenPosition = Camera.main.WorldToScreenPoint(playerIndicator.playerTransform.position);
            Vector3 finalScreenPosition = new Vector3();
            
            if (rawScreenPosition.z >= 0) // if in front of camera 
            {
                // Simply clamp it to the screen size so it's always visible
                finalScreenPosition.x = Mathf.Clamp(rawScreenPosition.x, 0, Screen.width);
                finalScreenPosition.y = Mathf.Clamp(rawScreenPosition.y, 0, Screen.height);
            }
            else // if behind the camera
            {
                float xMidpoint = Screen.width / 2;
                if (rawScreenPosition.x > xMidpoint) // if behind on the left side
                {
                    finalScreenPosition.x = 0; // clamp to left side of screen
                }
                else
                {
                    finalScreenPosition.x = Screen.width; // clamp to right side of screen
                }

                float yMidpoint = Screen.height / 2;
                if (rawScreenPosition.y > yMidpoint) // if behind on lower half
                {
                    finalScreenPosition.y = 0; // clamp to bottom of screen
                }
                else
                {
                    finalScreenPosition.y = Screen.height; // clamp to top of screen
                }
            }
            
            playerIndicator.indicator.position = finalScreenPosition;
        }
    }
    
    // Show indicator for a specific player
    public void ShowIndicatorForPlayer(ulong networkId, float duration, float fadeSpeed)
    {
        if (!playerIndicators.TryGetValue(networkId, out PlayerIndicator indicator))
        {
            Debug.LogWarning($"No indicator found for player with id {networkId}");
            return;
        }
        
        // Stop any existing showcase coroutine
        if (indicator.activeShowCallCoroutine != null)
        {
            StopCoroutine(indicator.activeShowCallCoroutine);
        }
        
        // Start new showcase coroutine
        indicator.activeShowCallCoroutine = StartCoroutine(DisplayShowCallIndicator(indicator.indicator, duration, fadeSpeed));
    }
    
    // Display the indicator for a specific duration and then fade it
    private IEnumerator DisplayShowCallIndicator(RectTransform indicator, float duration, float fadeSpeed)
    {
        indicator.localScale = Vector3.one; // Make visible
        yield return new WaitForSeconds(duration);
        
        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeSpeed)
        {
            float t = elapsed / fadeSpeed;
            indicator.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        indicator.localScale = Vector3.zero; // Ensure it's completely invisible
    }
}