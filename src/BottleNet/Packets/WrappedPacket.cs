using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BottleNet.Packets
{
    public class WrappedPacket
    {
        [JsonProperty("senderID")]
        public long SenderId { get; set; }
        
        [JsonProperty("packetId")]
        public string PacketType { get; set; }

        [JsonProperty("data")]
        public JToken PacketData { get; set; }

        public object GetPacket()
        {
            var split = PacketType.Split('.');
            var assembly = Assembly.Load(split[0]);
            var type = assembly.GetType(PacketType);
            return PacketData.ToObject(type);
        }

        public static WrappedPacket FromJson(string json)
        {
            return JToken.Parse(json).ToObject<WrappedPacket>();
        }
    }
}