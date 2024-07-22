using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    /// <summary>
    /// 基金持有人结构
    /// </summary>
    public class FundHolderStructure
    {
        public string FundCode { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// 机构持有占比
        /// </summary>
        public decimal OrgRatio { get; set; }

        /// <summary>
        /// 个人持有占比
        /// </summary>
        public decimal PersonalRatio { get; set; }

        /// <summary>
        /// 内部持有占比
        /// </summary>
        public decimal InnerRatio { get; set; }

        /// <summary>
        /// 总份额
        /// </summary>
        public decimal Share { get;set; }
    }
}
