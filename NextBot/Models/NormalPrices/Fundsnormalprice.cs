namespace NextBot.Models
{
    public partial class NormalPrices
    {
        public class Fundsnormalprice
        {
            public Fund fund { get; set; }
            public float[] normalPrices { get; set; }
            public float totalReturn { get; set; }
        }
    }
}
