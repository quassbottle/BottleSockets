using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace BottleNet.Packets
{
    public class StringPacket : Packet
    {
        [JsonProperty("content")]
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}