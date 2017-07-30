using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

using NetSystem = UnityEngine.Networking.NetworkSystem;

namespace sugi.cc
{
    public class NetworkMessageManager : MonoBehaviour
    {
        #region----Custom MessageBase----
        public static NetSystem.EmptyMessage GetEmptyMessage() { if (_emptyMessage == null) _emptyMessage = new NetSystem.EmptyMessage(); return _emptyMessage; }
        static NetSystem.EmptyMessage _emptyMessage;

        public static NetSystem.IntegerMessage GetIntegerMessage(int value) { if (_integerMessage == null) _integerMessage = new NetSystem.IntegerMessage(); _integerMessage.value = value; return _integerMessage; }
        static NetSystem.IntegerMessage _integerMessage;

        public static NetSystem.StringMessage GetStringMessage(string value) { if (_stringMessage == null) _stringMessage = new NetSystem.StringMessage(); _stringMessage.value = value; return _stringMessage; }
        static NetSystem.StringMessage _stringMessage;

        public class FloatMessage : MessageBase { public float value; }
        public static FloatMessage GetFloatMessage(float value) { if (_floatMessage == null) _floatMessage = new FloatMessage(); _floatMessage.value = value; return _floatMessage; }
        static FloatMessage _floatMessage;

        public class Vector3Message : MessageBase { public Vector3 value; }
        public static Vector3Message GetVector3Message(Vector3 value) { if (_vector3Message == null) _vector3Message = new Vector3Message(); _vector3Message.value = value; return _vector3Message; }
        static Vector3Message _vector3Message;

        public class BoolMessage : MessageBase { public bool value; }
        public static BoolMessage GetBoolMessage(bool value) { if (_boolMessage == null) _boolMessage = new BoolMessage(); _boolMessage.value = value; return _boolMessage; }
        static BoolMessage _boolMessage;
        #endregion

        [System.Serializable]
        public class NetworkMessageEvent : UnityEngine.Events.UnityEvent<NetworkMessage> { }

        #region MonoBehaviourFunc
        public NetworkMessageEvent registeredHandlers;
        void Start()
        {
            var numHandlers = registeredHandlers.GetPersistentEventCount();
            for (var i = 0; i < numHandlers; i++)
            {
                var target = registeredHandlers.GetPersistentTarget(i);
                var methodName = registeredHandlers.GetPersistentMethodName(i);
                var handler = (NetworkMessageDelegate)System.Delegate.CreateDelegate(typeof(NetworkMessageDelegate), target, methodName);
                AddHandler(handler);
            }

            NetworkManager.Instance.onStartServer.AddListener(RegisterHandlerToServer);
            NetworkManager.Instance.onClientConnect.AddListener((NetworkConnection conn) => { RegisterHandlerToClient(); });
        }
        #endregion

        static NetworkClient client { get { return NetworkManager.Instance.client; } }
        static bool showInfo;

        static string GetIdentifier(NetworkMessageDelegate handler) { return handler.Target.ToString() + handler.Method.Name; }

        static List<NetworkMessageDelegate> handlerList;
        static Dictionary<short, NetworkMessageDelegate> handlerMap;

        public static void AddHandler(NetworkMessageDelegate handler)
        {
            if (handlerList == null) handlerList = new List<NetworkMessageDelegate>();
            if (!handlerList.Contains(handler)) handlerList.Add(handler);
        }

        static void RegisterHandlerToServer()
        {
            if (handlerList == null) return;
            var msgType = MsgType.Highest;
            handlerMap = handlerList.OrderBy(handler => GetIdentifier(handler)).ToDictionary(b => ++msgType, b => b);
            foreach (var pair in handlerMap)
            {
                NetworkServer.RegisterHandler(pair.Key, pair.Value);
            }
            SettingManager.AddExtraGuiFunc(ShowNetworkMessageInfo);
        }
        static void RegisterHandlerToClient()
        {
            if (handlerList == null) return;
            var msgType = MsgType.Highest;
            handlerMap = handlerList.OrderBy(handler => GetIdentifier(handler)).ToDictionary(b => ++msgType, b => b);
            foreach (var pair in handlerMap)
            {
                client.RegisterHandler(pair.Key, pair.Value);
            }
            SettingManager.AddExtraGuiFunc(ShowNetworkMessageInfo);
        }

        public static void SendNetworkMessage(NetworkMessageDelegate handler, MessageBase message)
        {
            SendNetworkMessage(client.connection, handler, message);
        }

        public static void SendNetworkMessage(NetworkConnection conn, NetworkMessageDelegate handler, MessageBase message)
        {
            var msgType = handlerMap.Where(b => b.Value == handler).FirstOrDefault().Key;
            conn.Send(msgType, message);
        }

        public static void SendNetworkMessageToAll(NetworkMessageDelegate handler, MessageBase message)
        {
            var msgType = handlerMap.Where(b => b.Value == handler).FirstOrDefault().Key;
            NetworkServer.SendToAll(msgType, message);
        }

        static void ShowNetworkMessageInfo()
        {
            GUILayout.BeginVertical("box");
            showInfo = GUILayout.Toggle(showInfo, "show registered NetworkMessage Info?");
            if (showInfo)
            {
                foreach (var pair in handlerMap)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(pair.Key.ToString("000") + ":");
                    GUILayout.Label(GetIdentifier(pair.Value));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }
    }
}