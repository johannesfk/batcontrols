using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField ipAddressField;
    [SerializeField] private TMP_InputField portField;
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    
    private UnityTransport transport;
    
    private void Awake()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        // Default values
        ipAddressField.text = "127.0.0.1";
        portField.text = "7777";
        
        // Setup button listeners
        hostButton.onClick.AddListener(() => {
            SetConnectionDetails();
            NetworkManager.Singleton.StartHost();
            UpdateConnectionStatus("Host started on " + ipAddressField.text + ":" + portField.text);
        });
        
        clientButton.onClick.AddListener(() => {
            SetConnectionDetails();
            NetworkManager.Singleton.StartClient();
            UpdateConnectionStatus("Connecting to " + ipAddressField.text + ":" + portField.text + "...");
        });
        
        // Subscribe to connection events
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    
    private void SetConnectionDetails()
    {
        ushort port = ushort.Parse(portField.text);
        transport.ConnectionData.Address = ipAddressField.text;
        transport.ConnectionData.Port = port;
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost && clientId != NetworkManager.Singleton.LocalClientId)
        {
            UpdateConnectionStatus("Client connected: " + clientId);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            UpdateConnectionStatus("Connected to host!");
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost && clientId != NetworkManager.Singleton.LocalClientId)
        {
            UpdateConnectionStatus("Client disconnected: " + clientId);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            UpdateConnectionStatus("Disconnected from host!");
        }
    }
    
    private void UpdateConnectionStatus(string status)
    {
        connectionStatusText.text = status;
        Debug.Log(status);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events when object is destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}