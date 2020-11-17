using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BottleNet.Packets;
using BottleNet;
using BottleNet.Client;

namespace BottleClientExample
{
    class Program
    {
        static void Main()
        {
            Console.Title = "User";
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var client = new BottleClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000)); // локальный IP адрес
            
            client.DataReceived += ClientOnDataReceived;
            
            int tries = 0;
            while (!client.Connect())
            {
                tries++;
                Console.WriteLine("Попытка подключения  #" + tries);
                await Task.Delay(1000);
                Console.Clear();
            }
            Console.Clear();
            Console.WriteLine("Вы подключились к серверу!");

            client.SendPacket(new StringPacket()
            {
                Value = "Привет! Я только что подключился к серверу!"
            });
            
            await Task.Delay(-1);
        }
        
        private static void ClientOnDataReceived(object invoker, byte[] data)
        {
            string json = Encoding.UTF8.GetString(data); // получаем строку из массива байтов
            var wrappedPacket = WrappedPacket.FromJson(json); // получаем WrappedPacket, используя полученную строку ответа сервера
            var packet = wrappedPacket.GetPacket() as ResponsePacket;
            Console.WriteLine($"Пакет был принят: {(packet.Accepted ? "да" : "нет")}");
        }
    }
}