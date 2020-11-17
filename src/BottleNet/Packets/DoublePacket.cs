using Newtonsoft.Json;

namespace BottleNet.Packets
{
    public class DoublePacket : Packet
    {
        [JsonProperty("value")]
        public double Value { get; set; }
    }
}