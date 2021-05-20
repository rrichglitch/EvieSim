using MLAPI;
using UnityEngine;
using MLAPI.Transports.UNET;
using System.Threading.Tasks;

public class Pasta : MonoBehaviour
{
    public UNetTransport transport;
    private string connectAddress = "127.0.0.1";
    string natInfo = "";
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        if (GUILayout.Button("Host")){
            UPnPMngr.ForwardPort(transport.ServerListenPort);
            NetworkManager.Singleton.StartHost();
        }

        connectAddress = GUILayout.TextField(connectAddress, 15);
        transport.ConnectAddress = connectAddress;

        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        GUILayout.TextField(natInfo, 40);
        string[] names = QualitySettings.names;
        GUILayout.Space(30);
        GUILayout.BeginVertical();
        for (int i = 0; i < names.Length; i++)
        {
            if (GUILayout.Button(names[i]))
            {
                QualitySettings.SetQualityLevel(i, true);
            }
        }
        GUILayout.EndVertical();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
}