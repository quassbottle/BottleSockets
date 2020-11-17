using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BottleNet.Packets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BottleNet.Client
{
    public delegate void ServerConnectionArgs(object invoker);

    public delegate void ClientDataReceivedArgs(object invoker, byte[] data);
    
    public class BottleClient
    {
        private readonly Socket _clientSocket;
        private readonly IPEndPoint _ipEndPoint;
        
        public long? Id { get; private set; }
        public string Username { get; set; }

        public event ClientDataReceivedArgs DataReceived;
        
        public BottleClient(IPEndPoint endPoint)
        {
            _ipEndPoint = endPoint;
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public BottleClient(IPAddress address, int port) : this(new IPEndPoint(address, port))
        {
        }

        public bool Connect()
        {
            try
            {
                _clientSocket.Connect(_ipEndPoint);
                new Thread(new ParameterizedThreadStart(ReceiveResponseLoop)).Start(_clientSocket);
            }
            catch (SocketException e)
            {
                return false;
            }
            
            return true;
        }

        public bool Disconnect(bool reuseSocket)
        {
            try
            {
                _clientSocket.Disconnect(reuseSocket);
            }
            catch (SocketException)
            {
                return false;
            }
            return true;
        }

        public bool SendData(byte[] data)
        {
            try
            {
                _clientSocket.Send(data);
            }
            catch (SocketException)
            {
                return false;
            }
            return true;
        }

        public bool SendPacket(object packet)
        {
            if (!(packet is Packet))
            {
                throw new ArgumentException("Object is not type of Packet");
            }
            
            var wrap = new WrappedPacket()
            {
                SenderId = Id.GetValueOrDefault(),
                PacketType = packet.GetType().ToString(),
                PacketData = JObject.FromObject(packet) 
            };

            return SendData(Encoding.UTF8.GetBytes(JObject.FromObject(wrap).ToString()));
        }

        public void Shutdown()
        {
            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Close();
        }

        private void ReceiveResponseLoop(object obj)
        {
            var clientSocket = obj as Socket;
            while (clientSocket.Connected)
            {
                var buffer = new byte[32768];
                int received = clientSocket.Receive(buffer, SocketFlags.None);
                if (received == 0)
                    continue;
                var data = new byte[received];
                Array.Copy(buffer, data, received);
                if (Id == null)
                {
                    var response = Encoding.UTF8.GetString(data).Split(':');
                    long outId = -1;
                    long.TryParse(response[0], out outId);
                    if (outId != -1)
                    {
                        Id = outId;
                        Username = response[1];
                    }
                    continue;
                }
                
                DataReceived?.Invoke(this, data);
            }
        }
    }
}