using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextBot.Models
{
    public class Person
    {
        public Person()
        {
        }
        [Key]
        public long Id { get; set; }
        public long ChatId { get; set; }

        //public SmartPortfolioSetting SmartPortfolioSetting { get; set; }
        // instead of smartportfoliosetting
        public bool Save { get; set; }
        public long RiskRate { get; set; }
        public double MaximumStockWeight { get; set; }
        public double MinimumStockWeight { get; set; }
        public string ProductionDate { get; set; }


        public long ClassicNextSelectState { get; set; } // can be 1/21/41/...
        public long PortfolioIdForClassicNextSelect { get; set; }
        public int TickerKeyForStock { get; set; }

        public int CommandState { get; set; }
        public int CommandLevel { get; set; }

        public string StartDateWaitingForEndDate { get; set; }

        public int CreateSmartPortfolioType { get; set; }

    }
}
