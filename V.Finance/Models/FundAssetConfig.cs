using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class FundAssetConfig
    {
        public string FundCode { get; set; }

        public DateTime Date { get; set; }

        public decimal StockRatio { get; set; }

        public decimal BondRatio { get; set; }

        public decimal CashRatio { get; set; }

        public decimal NetAsset { get; set; }
    }
}
