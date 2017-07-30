using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using MessagePack;

namespace sugi.cc
{
    public enum WebsocketDataType { Json, MessagePack }
    public abstract class WebSocketDataBehaviour<T> : WebSocketDataBehaviour
    {
        public static WebSocketDataBehaviour<T> Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = FindObjectOfType<WebSocketDataBehaviour<T>>();
                return _Instance;
            }
        }
        static WebSocketDataBehaviour<T> _Instance;

        public string remoteAddress = "localhost";
        public int remotePort = 3000;
        public string path = "/";
        public WebsocketDataType dataType;

        WebSocket ws;
        protected Queue<T> receivedData { get { return WebsocketDataServer.DataGetter<T>.recievedData; } }

        private void Start()
        {
            if (Instance != this)
                Destroy(gameObject);
        }
        private void OnDestroy()
        {
            if (ws == null) return;
            ws.Close();
            ws = null;
        }

        void ConnectToServer()
        {
            if (ws == null)
            {
                var url = string.Format("ws://{0}:{1}{2}", remoteAddress, remotePort, path);
                Debug.Log(url);
                ws = new WebSocket(url);
            }
            ws.Connect();
        }

        public override void AddService()
        {
            WebsocketDataServer.Instance.AddService<T>(path);
        }

        #region onClient
        public void SendData(T data)
        {
            if (ws == null || ws.ReadyState != WebSocketState.Open)
                ConnectToServer();
            if (dataType == WebsocketDataType.Json)
            {
                var json = JsonUtility.ToJson(data);
                ws.SendAsync(json, OnSendCompleted);
            }
            else
            {
                var bytes = MessagePackSerializer.Serialize(data);
                ws.SendAsync(bytes, OnSendCompleted);
            }
        }
        void OnSendCompleted(bool completed)
        {
            if (completed)
                print("send completed");
            else
                print("send error");
        }
        #endregion
    }

    public abstract class WebSocketDataBehaviour : MonoBehaviour
    {
        public abstract void AddService();
    }
}