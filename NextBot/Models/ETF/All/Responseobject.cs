namespace NextBot.Models.ETF
{
    public partial class All
    {
        public class Responseobject
        {
            public int tickerKey { get; set; }
            public string isin { get; set; }
            public string symbol { get; set; }
            public string name { get; set; }
            public string marketType { get; set; }
        }
    }
}
