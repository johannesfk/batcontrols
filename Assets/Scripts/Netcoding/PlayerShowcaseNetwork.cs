using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This script handles the networking for player showcalls
public class PlayerShowcaseNetwork : NetworkBehaviour
{
    // Reference to the ModifiedShowCallRenderer component
    private ModifiedShowCallRenderer showCallRenderer;

    // Duration of the showcall
    public float showCallDuration = 3f;
    public float fadeSpeed = 1f;
    
    // List to track all active players in the game
    private static List<PlayerShowcaseNetwork> activePlayers = new List<PlayerShowcaseNetwork>();
    
    // The local player's instance
    private static PlayerShowcaseNetwork localPlayerInstance;

    private void Awake()
    {
        // Get the renderer component
        showCallRenderer = GetComponent<ModifiedShowCallRenderer>();
        
        // If not found, try to find it in children
        if (showCallRenderer == null)
        {
            showCallRenderer = GetComponentInChildren<ModifiedShowCallRenderer>();
        }
        
        // Log error if still not found
        if (showCallRenderer == null)
        {
            Debug.LogError("ModifiedShowCallRenderer component not found!");
        }
    }

    public override void OnNetworkSpawn()
    {
        // Add this player to the active players list
        activePlayers.Add(this);
        
        // If this is the local player, store a reference to it
        if (IsOwner)
        {
            localPlayerInstance = this;
            
            // Initialize ShowCallRenderer for local player
            InitializeShowCallRenderer();
        }
        
        // When a new player spawns, we need to update all existing players' renderers
        UpdateAllPlayerRenderers();
        
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        // Remove from active players list
        activePlayers.Remove(this);
        
        // If this is the local player, clear the reference
        if (IsOwner)
        {
            localPlayerInstance = null;
        }
        
        // Update all remaining players' renderers
        UpdateAllPlayerRenderers();
        
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        // Only process input for the local player
        if (!IsOwner) return;

        // Check for the show call input
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Call the server RPC to inform all clients about the showcall
            ShowCallServerRpc(NetworkObjectId);
        }
    }

    // Initialize the ShowCallRenderer with references to other players
    private void InitializeShowCallRenderer()
    {
        if (showCallRenderer == null) return;
        
        // Create the player indicators for each other player
        UpdatePlayerIndicators();
    }
    
    // Update indicators for all players
    private void UpdatePlayerIndicators()
    {
        if (showCallRenderer == null) return;
        
        // Get a list of all other players (not this one)
        List<PlayerShowcaseNetwork> otherPlayers = new List<PlayerShowcaseNetwork>();
        foreach (var player in activePlayers)
        {
            if (player != this)
            {
                otherPlayers.Add(player);
            }
        }
        
        // Update the renderer with the other players
        showCallRenderer.SetupPlayerIndicators(otherPlayers);
    }
    
    // Update all players' renderers
    private static void UpdateAllPlayerRenderers()
    {
        foreach (var player in activePlayers)
        {
            if (player.IsOwner)
            {
                player.UpdatePlayerIndicators();
            }
        }
    }

    [ServerRpc]
    private void ShowCallServerRpc(ulong callerNetworkObjectId)
    {
        // Server receives the call and broadcasts it to all clients
        ShowCallClientRpc(callerNetworkObjectId);
    }

    [ClientRpc]
    private void ShowCallClientRpc(ulong callerNetworkObjectId)
    {
        // Don't process if we're the caller
        if (NetworkObjectId == callerNetworkObjectId) return;
        
        // Display the showcall indicator on the client
        if (IsOwner && showCallRenderer != null)
        {
            showCallRenderer.ShowIndicatorForPlayer(callerNetworkObjectId, showCallDuration, fadeSpeed);
        }
    }
    
    // Public method to get this player's transform
    public Transform GetPlayerTransform()
    {
        return transform;
    }
    
    // Public method to get this player's NetworkObjectId
    public ulong GetNetworkObjectId()
    {
        return NetworkObjectId;
    }
}