using Newtonsoft.Json;

namespace BottleNet.Packets
{
    public class IntegerPacket : Packet
    {
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}