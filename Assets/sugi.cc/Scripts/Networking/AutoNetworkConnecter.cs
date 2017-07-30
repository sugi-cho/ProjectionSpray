using UnityEngine;
using UnityEngine.Networking;

namespace sugi.cc
{
    /// <summary>
    /// this class is for Recconection sugi.cc.NetworkManager.
    /// </summary>
    public class AutoNetworkConnecter : MonoBehaviour
    {
        public float reconnectDuration = 5f;
        NetworkManager manager { get { return NetworkManager.Instance; } }
        void Start()
        {
            NetworkManager.Instance.onServerError.AddListener(OnError);
            NetworkManager.Instance.onClientError.AddListener(OnError);
            NetworkManager.Instance.onClientDisconnect.AddListener((NetworkConnection conn) => { Connect(); });
            if (0f < reconnectDuration)
                this.Invoke(Connect, reconnectDuration);
        }

        void OnError(NetworkConnection conn, NetworkError netError)
        {
            if (netError == NetworkError.Timeout)
                Connect();
        }

        public void Connect()
        {
            if (manager.isNetworkActive)
                return;
            if (manager.setting.isServer)
            {
                if (manager.setting.isClient)
                    manager.StartHost();
                else
                    manager.StartServer();
            }
            else if (manager.setting.isClient)
                manager.StartClient();
            else if (0f < reconnectDuration)
                this.Invoke(Connect, reconnectDuration);
            else
                Debug.Log("No Reconnection!");
        }
    }
}