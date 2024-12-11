using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class FundNav
    {
        public string FundCode { get; set; }

        /// <summary>
        /// 单位净值(货币基金-万份收益)
        /// </summary>
        public decimal UnitNav { get; set; }

        /// <summary>
        /// 累计净值(货币基金-七日年化)
        /// </summary>
        public decimal AccUnitNav { get; set; }

        public DateTime Date { get; set; }
    }
}
