using System.Text.Json.Serialization;

namespace NextBot.Models
{
    public partial class IndustryStocks
    {
        public class Stock
        {
            [JsonPropertyName("tickerKey")]
            public int TickerKey { get; set; }
            [JsonPropertyName("symbol")]
            public string Symbol { get; set; }
            [JsonPropertyName("exchangeType")]
            public string ExchangeType { get; set; }
        }
    }
}
