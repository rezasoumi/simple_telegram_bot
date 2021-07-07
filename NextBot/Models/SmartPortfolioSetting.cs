namespace NextBot.Models
{
    public class SmartPortfolioSetting
    {
        public long Id { get; set; }
        public bool Save { get; set; }
        public long RiskRate { get; set; }
        public double MaximumStockWeight { get; set; }
        public double MinimumStockWeight { get; set; }
        public string ProductionDate { get; set; }
    }
}