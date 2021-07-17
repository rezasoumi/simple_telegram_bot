using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextBot.Models
{
    public partial class PortfolioSet
    {
        public class AddPortfolioParameter
        {
            public long PortfolioSetId { get; set; }
            public int[] PortfolioIds { get; set; }
        }
    }
}
