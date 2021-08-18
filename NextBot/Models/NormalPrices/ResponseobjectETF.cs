using System;

namespace NextBot.Models
{
    public partial class NormalPrices
    {
        public class ResponseobjectETF
        {
            public DateTime[] dates { get; set; }
            public string[] datesPersian { get; set; }
            public Fundsnormalprice[] fundsNormalPrices { get; set; }
        }
    }
}
