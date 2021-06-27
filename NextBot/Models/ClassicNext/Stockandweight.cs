using System.Text.Json.Serialization;

namespace NextBot.Models
{
    public class Stockandweight
    {
        [JsonPropertyName("stock")]
        public Stock Stock { get; set; }
        [JsonPropertyName("weight")]
        public float Weight { get; set; }
    }

}
