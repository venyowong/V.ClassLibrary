using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class IndexConstituent
    {
        public string IndexCode { get; set; }

        public string StockCode { get; set; }

        public string StockName { get; set; }

        public decimal Weight { get; set; }

        public DateTime Begin { get; set; }
    }
}
