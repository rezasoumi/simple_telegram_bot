namespace NextBot.Models
{
    public partial class NormalPrices
    {
        public class Stocksnormalprice
        {
            public Stock stock { get; set; }
            public float[] normalPrices { get; set; }
            public float totalReturn { get; set; }
        }
    }
}
