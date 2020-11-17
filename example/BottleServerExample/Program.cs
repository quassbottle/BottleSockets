using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BottleNet;
using BottleNet.Packets;
using BottleNet.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BottleServerDebug
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Server";
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var server = new BottleServer(8000);
            server.ClientConnected += ServerOnClientConnected;
            server.Received += ServerOnReceived;
            server.ClientDisconnected += ServerOnClientDisconnected;
            server.Start();
            Console.WriteLine("Сервер запущен!");
            await Task.Delay(-1);
        }

        private static void ServerOnClientDisconnected(object invoker, User sender)
        {
            Console.WriteLine($"Пользователь \"{sender.Username}\" отключился!");
        }

        private static bool ServerOnReceived(object invoker, User sender, byte[] data)
        {
            string receivedText = Encoding.UTF8.GetString(data); // получаем текст запроса пользователя
            var wrappedPacket = WrappedPacket.FromJson(receivedText); //JToken.Parse(Encoding.UTF8.GetString(data)).ToObject(typeof(WrappedPacket)) as WrappedPacket;
            if (wrappedPacket.PacketType == typeof(StringPacket).ToString())
            {
                Console.WriteLine($"{sender.Username}: {(wrappedPacket.GetPacket() as StringPacket).Value}");
                return true; // возвращаем true, так как пакет принят (в ResponsePacket вернет значение, которое мы здесь возвращаем, т. е. в данном случае true)
            }
            else
            {
                return false; // мы получили не то, что ожидали, поэтому возвращаем false
            }
        }

        private static void ServerOnClientConnected(object invoker, User current)
        {
            Console.WriteLine("Пользователь \"" + current.Username + "\" подключился к серверу!");
        }
    }
}