namespace NextBot.Models
{
    public partial class NormalPrices
    {
        public class Stock
        {
            public int tickerKey { get; set; }
            public string isin { get; set; }
            public string tickerPooyaFa { get; set; }
            public string tickerNamePooyaFa { get; set; }
            public int exchangeKey { get; set; }
            public object marketUnit { get; set; }
            public string marketType { get; set; }
        }
    }
}
