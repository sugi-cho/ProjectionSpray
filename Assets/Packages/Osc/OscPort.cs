using System.Net.Sockets;
using System;
using System.Net;
using Osc;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System.Threading;

namespace Osc
{
    public abstract class OscPort : MonoBehaviour
    {
        public const int BUFFER_SIZE = 1 << 16;
        public CapsuleEvent OnReceive;
        public ExceptionEvent OnError;

        public int localPort = 0;
        public string defaultRemoteHost = "localhost";
        public int defaultRemotePort = 10000;
        public int limitReceiveBuffer = 10;

        protected Parser _oscParser;
        protected Queue<Capsule> _received;
        protected Queue<System.Exception> _errors;
        protected IPEndPoint _defaultRemote;

        protected virtual void OnEnable()
        {
            _oscParser = new Parser();
            _received = new Queue<Capsule>();
            _errors = new Queue<Exception>();
            _defaultRemote = new IPEndPoint(FindFromHostName(defaultRemoteHost), defaultRemotePort);
        }
        protected virtual void OnDisable()
        {
        }

        protected virtual void Update()
        {
            lock (_received)
                while (_received.Count > 0)
                    OnReceive.Invoke(_received.Dequeue());
            lock (_errors)
                while (_errors.Count > 0)
                    OnError.Invoke(_errors.Dequeue());
        }

        public void Send(MessageEncoder oscMessage)
        {
            Send(oscMessage, _defaultRemote);
        }
        public void Send(MessageEncoder oscMessage, IPEndPoint remote)
        {
            Send(oscMessage.Encode(), remote);
        }
        public void Send(byte[] oscData)
        {
            Send(oscData, _defaultRemote);
        }
        public abstract void Send(byte[] oscData, IPEndPoint remote);

        public IPAddress FindFromHostName(string hostname)
        {
            var addresses = Dns.GetHostAddresses(hostname);
            IPAddress address = IPAddress.None;
            for (var i = 0; i < addresses.Length; i++)
            {
                if (addresses[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    address = addresses[i];
                    break;
                }
            }
            return address;
        }
        protected void RaiseError(System.Exception e)
        {
            _errors.Enqueue(e);
        }
        protected void Receive(OscPort.Capsule c)
        {
            if (limitReceiveBuffer <= 0 || _received.Count < limitReceiveBuffer)
                _received.Enqueue(c);
        }

        public struct Capsule
        {
            public Message message;
            public IPEndPoint ip;

            public Capsule(Message message, IPEndPoint ip)
            {
                this.message = message;
                this.ip = ip;
            }
        }
    }

    [System.Serializable]
    public class ExceptionEvent : UnityEvent<Exception> { }
    [System.Serializable]
    public class CapsuleEvent : UnityEvent<OscPort.Capsule> { }
    [System.Serializable]
    public class MessageEvent : UnityEvent<Message> { }
}