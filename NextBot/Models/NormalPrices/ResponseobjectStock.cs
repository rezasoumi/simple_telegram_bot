using System;

namespace NextBot.Models
{
    public partial class NormalPrices
    {
        public class ResponseobjectStock
        {
            public DateTime[] dates { get; set; }
            public string[] datesPersian { get; set; }
            public Stocksnormalprice[] stocksNormalPrices { get; set; }
        }
    }
}
