﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextBot.Models
{
    public partial class NormalPrices
    {
        public class RootObjectStock
        {
            public ResponseobjectStock responseObject { get; set; }
            public bool isSuccessful { get; set; }
            public string errorMessageFa { get; set; }
            public string errorMessage { get; set; }
        }
    }
}
