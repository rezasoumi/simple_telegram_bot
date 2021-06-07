namespace NextBot.Models
{
    public partial class IndustryStocks
    {
        public class Stock
        {
            public int tickerKey { get; set; }
            public string symbol { get; set; }
            public string exchangeType { get; set; }
        }

    }
}
