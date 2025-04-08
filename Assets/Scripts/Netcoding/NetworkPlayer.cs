using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerVisual;
    [SerializeField] private float moveSpeed = 5f;
    
    // Network variable for position syncing
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    // Network variable for rotation syncing    
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    public override void OnNetworkSpawn()
    {
        // If this is the local player, change the color to blue
        if (IsOwner)
        {
            // Set local player color to blue
            if (playerVisual != null && playerVisual.GetComponent<Renderer>() != null)
            {
                playerVisual.GetComponent<Renderer>().material.color = Color.blue;
            }
        }
        else
        {
            // Set other players' color to red
            if (playerVisual != null && playerVisual.GetComponent<Renderer>() != null)
            {
                playerVisual.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }
    
    private void Update()
    {
        // Only process inputs for the local player
        if (IsOwner)
        {
            HandleMovement();
            UpdateNetworkVariables();
        }
        else
        {
            // For non-owners, apply the networked position/rotation (with smoothing)
            transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation.Value, Time.deltaTime * 10f);
        }
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;
        
        // Simple rotation based on movement direction
        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movement);
        }
    }
    
    private void UpdateNetworkVariables()
    {
        // Update network position and rotation
        networkPosition.Value = transform.position;
        networkRotation.Value = transform.rotation;
    }

    [ServerRpc]
    private void RequestActionServerRpc()
    {
        // This runs on the server when called by a client
        Debug.Log($"Player {OwnerClientId} requested an action!");
        
        // You can perform server-side validation here
        
        // Then notify all clients about the action
        NotifyActionClientRpc();
    }

    [ClientRpc]
    private void NotifyActionClientRpc()
    {
        // This runs on all clients when called by the server
        Debug.Log("Server approved an action!");
        
        // Perform the actual action on all clients
        // For example, play an animation, sound effect, etc.
    }

    // Call the ServerRPC when appropriate:
    // if (IsOwner && Input.GetKeyDown(KeyCode.Space))
    // {
    //     RequestActionServerRpc();
    // }
}