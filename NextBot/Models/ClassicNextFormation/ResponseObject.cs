using System.Text.Json.Serialization;

namespace NextBot.Models
{
    public partial class ClassicNextFormation
    {
        public class ResponseObject
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("stockAndWeights")]
            public Stockandweight[] StockAndWeights { get; set; }
        }
    }
}
