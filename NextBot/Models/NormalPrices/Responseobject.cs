using System;

namespace NextBot.Models
{
    public partial class NormalPrices
    {
        public class Responseobject
        {
            public DateTime[] dates { get; set; }
            public string[] datesPersian { get; set; }
            public float[] normalPrices { get; set; }
            public float totalReturn { get; set; }
        }
    }
}
