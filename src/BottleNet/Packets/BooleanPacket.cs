using Newtonsoft.Json;

namespace BottleNet.Packets
{
    public class BooleanPacket: Packet
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }
}