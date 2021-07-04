using System.Text.Json.Serialization;

namespace NextBot.Models
{
    public partial class ClassicNextFormation
    {
        public class Stock
        {
            [JsonPropertyName("tickerKey")]
            public int TickerKey { get; set; }
            [JsonPropertyName("isin")]
            public string Isin { get; set; }
            [JsonPropertyName("tickerPooyaFa")]
            public string TickerPooyaFa { get; set; }
            [JsonPropertyName("tickerNamePooyaFa")]
            public string TickerNamePooyaFa { get; set; }
            [JsonPropertyName("exchangeKey")]
            public int ExchangeKey { get; set; }
            [JsonPropertyName("marketUnit")]
            public object MarketUnit { get; set; }
            [JsonPropertyName("marketType")]
            public string MarketType { get; set; }
        }
    }
}
