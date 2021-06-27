using System.Text.Json.Serialization;

namespace NextBot.Models
{
    public partial class IndustryStocks
    {
        public class Industry
        {
            [JsonPropertyName("tickerKey")]
            public int TickerKey { get; set; }
            [JsonPropertyName("nameFa")]
            public string NameFa { get; set; }
            [JsonPropertyName("stocks")]
            public Stock[] Stocks { get; set; }
        }
    }
}
