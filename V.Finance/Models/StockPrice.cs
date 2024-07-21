using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class StockPrice
    {
        public string StockCode { get; set; }

        public DateTime Date { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public decimal Max { get; set; }

        public decimal Min { get; set; }
    }
}
