namespace NextBot.Models
{
    public partial class IndustryStocks
    {
        public class Industry
        {
            public int tickerKey { get; set; }
            public string nameFa { get; set; }
            public Stock[] stocks { get; set; }
        }

    }
}
