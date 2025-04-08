using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // When a client connects, spawn a player for them
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayerForClient;
            
            // Also spawn player for the host
            SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId);
        }
    }
    
    private void SpawnPlayerForClient(ulong clientId)
    {
        // Create a position with some offset based on client ID to avoid players spawning on top of each other
        Vector3 spawnPosition = new Vector3(clientId * 2f, 0f, 0f);
        
        // Spawn the player
        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        // Make it a networked object owned by the client
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayerForClient;
        }
    }
}