namespace NextBot.Models
{
    public partial class NormalPrices
    {
        public class Fund
        {
            public int tickerKey { get; set; }
            public string isin { get; set; }
            public string symbol { get; set; }
            public string name { get; set; }
            public string marketType { get; set; }
        }
    }
}
