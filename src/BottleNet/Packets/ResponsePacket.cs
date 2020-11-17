using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BottleNet.Packets
{
    public class ResponsePacket
    {
        [JsonProperty("accepted")]
        public bool Accepted { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; } = "";
        
        [JsonProperty("sentPacketType")]
        public string SentPacketType { get; set; }
        
        [JsonProperty("sentPacket")]
        public JToken SentPacketData { get; set; }
        
        public object GetSentPacket()
        {
            var split = SentPacketType.Split('.');
            var assembly = Assembly.Load(split[0]);
            var type = assembly.GetType(SentPacketType);
            return SentPacketData.ToObject(type);
        }
    }
}