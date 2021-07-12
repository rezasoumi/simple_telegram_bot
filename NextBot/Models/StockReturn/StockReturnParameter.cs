using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextBot.Models
{
    public partial class StockReturn
    {
        public class StockReturnParameter
        {
            public int[] TickerKeys { get; set; }
            public int BeginDatePersian { get; set; }
        }
    }
}
