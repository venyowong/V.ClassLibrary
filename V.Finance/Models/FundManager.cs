using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class FundManagerChange
    {
        public DateTime BeginDate { get;set; }

        public DateTime EndDate { get;set; }

        public List<string> Names {  get;set; }

        /// <summary>
        /// 任职时长
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// 任职回报，小数形式
        /// </summary>
        public decimal Yield { get; set; }
    }
}
