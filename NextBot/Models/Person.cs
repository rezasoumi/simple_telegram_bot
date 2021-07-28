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
            SmartPortfolioSetting = new SmartPortfolioSetting { };
        }
        [Key]
        public long Id { get; set; }
        public long ChatId { get; set; }
        public long State { get; set; }
        public SmartPortfolioSetting SmartPortfolioSetting { get; set; }
        public long ClassicNextSelectState { get; set; } // can be 1/21/41/...
        public long PortfolioIdForClassicNextSelect { get; set; }
        public bool GetSave { get; set; }
        public bool GetRisk { get; set; }
        public bool GetMinimumStockWeight { get; set; }
        public bool GetMaximumStockWeight { get; set; }
        public bool GetDate { get; set; }
        public int TickerKeyForStock { get; set; }

        public int CommandState { get; set; }
        public int CommandLevel { get; set; }

        public string StartDateWaitingForEndDate { get; set; }

        public int CreateSmartPortfolioType { get; set; }

    }
}
