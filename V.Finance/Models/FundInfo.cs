using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class FundInfo
    {
        public string FundFullName { get; set; }

        public string FundName { get; set; }

        public string FundCode { get; set; }

        public string FundType { get; set; }

        /// <summary>
        /// 发行日期
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// 成立日期
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// 管理人
        /// </summary>
        public string Administrator { get; set; }

        /// <summary>
        /// 托管人
        /// </summary>
        public string Trustee { get; set; }

        /// <summary>
        /// 基金经理
        /// </summary>
        public string Manager { get; set; }

        /// <summary>
        /// 资产规模
        /// </summary>
        public decimal Scale { get; set; }

        /// <summary>
        /// 份额规模
        /// </summary>
        public decimal Share { get; set; }

        /// <summary>
        /// 成立以来分红
        /// </summary>
        public decimal Bonus { get; set; }

        /// <summary>
        /// 管理费率
        /// </summary>
        public decimal? ManagementRate { get; set; }

        /// <summary>
        /// 托管费率
        /// </summary>
        public decimal? EscrowRate { get; set; }

        /// <summary>
        /// 销售服务费率
        /// </summary>
        public decimal? SaleServiceRate { get; set; }

        /// <summary>
        /// 最大认购费率
        /// </summary>
        public decimal MaxSubscriptionRate { get; set; }

        /// <summary>
        /// 最大申购费率
        /// </summary>
        public decimal MaxPurchaseRate { get; set; }

        /// <summary>
        /// 最大赎回费率
        /// </summary>
        public decimal MaxRedeemRate { get; set; }

        /// <summary>
        /// 业绩比较基准
        /// </summary>
        public string Benchmark { get; set; }

        /// <summary>
        /// 跟踪标的
        /// </summary>
        public string Tracking { get; set; }

        /// <summary>
        /// 投资目标
        /// </summary>
        public string InvestmentTarget { get; set; }

        /// <summary>
        /// 投资理念
        /// </summary>
        public string InvestmentPhilosophy { get; set; }

        /// <summary>
        /// 投资范围
        /// </summary>
        public string InvestmentScope { get; set; }

        /// <summary>
        /// 投资策略
        /// </summary>
        public string InvestmentStrategy { get; set; }

        /// <summary>
        /// 分红政策
        /// </summary>
        public string DividendPolicy { get; set; }

        /// <summary>
        /// 风险收益特征
        /// </summary>
        public string Risk { get; set; }
    }
}
