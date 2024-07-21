using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class FundRankModel
    {
        public string FundCode { get; set; }

        public string FundName { get; set;}

        public DateTime Date { get; set; }

        /// <summary>
        /// 近一年涨幅
        /// </summary>
        public decimal YieldYear1 { get; set; }
    }
}
