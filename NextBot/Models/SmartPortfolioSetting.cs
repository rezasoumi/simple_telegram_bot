using System.ComponentModel.DataAnnotations.Schema;

namespace NextBot.Models
{
    public class SmartPortfolioSetting
    {
        public SmartPortfolioSetting()
        {
        }
        public long Id { get; set; }
        public bool Save { get; set; }
        public long RiskRate { get; set; }
        public double MaximumStockWeight { get; set; }
        public double MinimumStockWeight { get; set; }
        public string ProductionDate { get; set; }
    }
}