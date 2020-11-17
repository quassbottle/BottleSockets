using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BottleNet.Packets;
using Newtonsoft.Json.Linq;

namespace BottleNet.Server
{
    public delegate bool ReceiveCallbackArgs(object invoker, User sender, byte[] data);
    public delegate void UserArgs(object invoker, User sender); // todo

    public class BottleServer
    {
        public List<User> ConnectedClients { get; }
        
        private readonly Socket _serverSocket;
        private readonly byte[] _buffer;
        private readonly int _bufferSize;
        private readonly IPEndPoint _ipEndPoint;
        
        public event ReceiveCallbackArgs Received;
        public event UserArgs ClientDisconnected;
        public event UserArgs ClientConnected;
        
        public BottleServer(int port, int bufferSize = 32768)
        {
            ConnectedClients = new List<User>();
            _bufferSize = bufferSize;
            _buffer = new byte[bufferSize];
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _ipEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public void Start()
        {
            _serverSocket.Bind(_ipEndPoint);
            _serverSocket.Listen(0);
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult result)
        {
            var socket = _serverSocket.EndAccept(result);
            var user = new User()
            {
                Socket = socket
            };
            user.Username = "u-" + user.Id;
            ConnectedClients.Add(user);
            ClientConnected?.Invoke(this, user);
            
            socket.BeginReceive(_buffer, 0, _bufferSize, SocketFlags.None, ReceiveCallback, new KeyValuePair<int, Socket>(user.Id, socket));
            socket.Send(Encoding.UTF8.GetBytes(user.Id + ":" + user.Username));
        }

        public User GetUser(int id)
        {
            return ConnectedClients.Find(u => u.Id == id);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            var currentResult = (result.AsyncState as KeyValuePair<int, Socket>?).GetValueOrDefault();

            int received = -1;

            try
            {
                received = currentResult.Value.EndReceive(result);
            }
            catch (SocketException)
            {
            }
            
            var currentUser = GetUser(currentResult.Key);
            var currentSocket = currentResult.Value;

            if (received == -1)
            {
                currentSocket.Shutdown(SocketShutdown.Both);
                ConnectedClients.RemoveAll(c => c.Socket.Equals(currentResult.Value));
                ClientDisconnected?.Invoke(this, currentUser);
                _serverSocket.BeginAccept(AcceptCallback, null);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);
            
            if (Received != null)
            {
                var responsePacket = new ResponsePacket();

                try
                {
                    var sentPacket = WrappedPacket.FromJson(Encoding.UTF8.GetString(recBuf)).GetPacket();
                    responsePacket.SentPacketData = JToken.FromObject(sentPacket);
                    responsePacket.SentPacketType = sentPacket.GetType().ToString();
                }
                catch
                {
                    responsePacket.SentPacketData = JToken.FromObject(new
                    {
                        content = Encoding.UTF8.GetString(recBuf)
                    });
                    responsePacket.SentPacketType = typeof(JToken).ToString();
                }

                responsePacket.Accepted = Received.Invoke(this, currentUser, recBuf);
                
                var wrappedPacket = new WrappedPacket()
                {
                    PacketType = responsePacket.GetType().ToString(),
                    PacketData = JObject.FromObject(responsePacket)
                };

                currentResult.Value.Send(Encoding.UTF8.GetBytes(JObject.FromObject(wrappedPacket).ToString()));
            }

            currentResult.Value.BeginReceive(_buffer, 0, _bufferSize, SocketFlags.None, ReceiveCallback, currentResult);
        }
    }
}