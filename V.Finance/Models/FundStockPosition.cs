using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class FundStockPosition
    {
        public string StockCode { get; set; }

        public string StockName { get; set; }

        public decimal Ratio { get; set; }

        /// <summary>
        /// 持股数
        /// </summary>
        public decimal Share { get; set; }

        public decimal MarketValue { get; set; }

        public DateTime ReportDate { get; set; }

        public string FundCode { get; set; }
    }
}
