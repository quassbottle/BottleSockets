using System.Net.Sockets;

namespace BottleNet
{
    public class User
    {
        public int Id => this.GetHashCode();
        public string Username { get; set; }
        public Socket Socket { get; set; }
    }
}