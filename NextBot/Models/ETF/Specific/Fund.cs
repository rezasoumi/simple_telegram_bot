namespace NextBot.Models.ETF
{
    public partial class Specific
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
