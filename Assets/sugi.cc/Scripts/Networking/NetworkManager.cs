using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace sugi.cc
{
    /// <summary>
    /// NetworkManager. this name is same as Unityengine.Networking.NetworkManager! but I want to use this name;)
    /// </summary>
    public class NetworkManager : UnityEngine.Networking.NetworkManager
    {
        public static NetworkManager Instance { get { if (_Instance == null) _Instance = FindObjectOfType<NetworkManager>(); return _Instance; } }
        static NetworkManager _Instance;

        [System.Serializable]
        public class ConnectEvent : UnityEvent<NetworkConnection> { }
        [System.Serializable]
        public class NetworkErrorEvent : UnityEvent<NetworkConnection, NetworkError> { }

        [SerializeField]
        public Setting setting { get; private set; }
        [SerializeField]
        string settingFilePath = "Networking/setting.json";
        [SerializeField]
        string[] networkPrefabResourcePathes = new[] { "Networking/Prefabs" };

        #region ----UnityEvents-----
        public UnityEvent onStartServer;
        public ConnectEvent onServerConnect;
        public ConnectEvent onServerDisconnect;
        public NetworkErrorEvent onServerError;

        public ConnectEvent onClientSceneChanged;
        public ConnectEvent onClientConnect;
        public ConnectEvent onClientDisconnect;
        public NetworkErrorEvent onClientError;
        #endregion

        // Use this for initialization
        void Start()
        {
            SettingManager.AddSettingMenu(setting, settingFilePath);
        }

        #region ----Server Callbacks----
        public override void OnStartServer()
        {
            base.OnStartServer();
            onStartServer.Invoke();
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);
            onServerConnect.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            onServerDisconnect.Invoke(conn);
        }

        public override void OnServerError(NetworkConnection conn, int errorCode)
        {
            base.OnServerError(conn, errorCode);
            var netError = (NetworkError)errorCode;
            Debug.Log("NetworkError: " + netError);
            onServerError.Invoke(conn, netError);
        }
        #endregion

        #region ----Client Callbacks----
        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            base.OnClientSceneChanged(conn);
            foreach (var path in networkPrefabResourcePathes)
            {
                var nets = Resources.LoadAll<NetworkIdentity>(path);
                foreach (var net in nets)
                    ClientScene.RegisterPrefab(net.gameObject);
            }
            onClientSceneChanged.Invoke(conn);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            onClientConnect.Invoke(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            onClientDisconnect.Invoke(conn);
        }

        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            base.OnClientError(conn, errorCode);
            var netError = (NetworkError)errorCode;
            Debug.Log("NetworkError: " + netError);
            onClientError.Invoke(conn, netError);
        }
        #endregion

        [System.Serializable]
        public class Setting : SettingManager.Setting
        {
            public bool isServer;
            public bool isClient;

            public string networkAddress = "localhost";
            public int networkPort = 7777;

            bool showConnections;

            protected override void OnLoad()
            {
                base.OnLoad();
                singleton.networkAddress = networkAddress;
                singleton.networkPort = networkPort;
            }
            public override void OnGUIFunc()
            {
                base.OnGUIFunc();

                var noConnection = (singleton.client == null || singleton.client.connection == null || singleton.client.connection.connectionId == -1);

                if (!singleton.IsClientConnected() && !NetworkServer.active && singleton.matchMaker == null)
                {
                    if (noConnection)
                    {
                        if (GUILayout.Button("LAN Host"))
                            singleton.StartHost();
                        if (GUILayout.Button("LAN Client"))
                            singleton.StartClient();
                        if (GUILayout.Button("LAN Server Only"))
                            singleton.StartServer();
                    }
                    else
                    {
                        GUILayout.Label("Connecting to " + singleton.networkAddress + ":" + singleton.networkPort + "..");
                        if (GUILayout.Button("Cancel Connection Attempt"))
                            singleton.StopClient();
                    }
                }
                else
                {
                    if (NetworkServer.active)
                        GUILayout.Label("Server: port=" + singleton.networkPort);
                    if (NetworkClient.active)
                        GUILayout.Label("Client: address=" + singleton.networkAddress + " port=" + singleton.networkPort);
                }

                if (NetworkServer.active || NetworkClient.active)
                    if (GUILayout.Button("Stop"))
                        singleton.StopHost();

                if (NetworkServer.active)
                {
                    GUILayout.BeginVertical("box");
                    if (showConnections = GUILayout.Toggle(showConnections, "show connections"))
                    {
                        foreach (var conn in NetworkServer.connections)
                        {
                            if (conn == null)
                            {
                                var tmp = GUI.contentColor;
                                GUI.contentColor = Color.red;
                                GUILayout.Label(string.Format("connection.#{0} is null", NetworkServer.connections.IndexOf(conn).ToString("00")));
                                GUI.contentColor = tmp;
                            }
                            else
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("address:" + conn.address);
                                GUILayout.Label("connection ID:" + conn.connectionId);
                                GUILayout.Label("isConnected:" + conn.isConnected);
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                else if (singleton.client != null && singleton.client.connection != null)
                {
                    GUILayout.BeginVertical("box");
                    if (showConnections = GUILayout.Toggle(showConnections, "show connection"))
                    {
                        var conn = singleton.client.connection;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("address:" + conn.address);
                        GUILayout.Label("connection ID:" + conn.connectionId);
                        GUILayout.Label("isConnected:" + conn.isConnected);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
            }

            protected override void OnClose()
            {
                base.OnClose();
                singleton.networkAddress = networkAddress;
                singleton.networkPort = networkPort;
            }
        }
    }
}