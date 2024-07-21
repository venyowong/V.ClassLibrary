using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using V.Common.Extensions;
using V.Finance.Models;

namespace V.Finance.Services
{
    public class FinancialService
    {
        /// <summary>
        /// 将年化收益率换算成月度收益率(复利)
        /// </summary>
        /// <param name="annualYield">年化收益率，小数点</param>
        /// <returns></returns>
        public double ConvertAnnualToMonthly(double annualYield)
        {
            return Math.Pow(1 + annualYield, 1 / 12) - 1;
            //return Math.Pow(Math.E, Math.Log(1 + annualYield) / 12) - 1;
        }

        /// <summary>
        /// 计算定投N期后的总资产
        /// </summary>
        /// <param name="yield">月度收益率</param>
        /// <param name="months">定投月数</param>
        /// <param name="investment">每月定投金额</param>
        /// <param name="asset">现有资产</param>
        /// <returns></returns>
        public decimal FixedInvest(decimal yield, int months, decimal investment, decimal asset)
        {
            for (int i = 0; i < months; i++)
            {
                asset = (asset + investment) * (1 + yield);
            }
            return asset;
        }

        public List<IncreaseRate> GetIncreaseRates(List<IndexQuotation> quotations)
        {
            if (quotations.IsNullOrEmpty())
            {
                return new List<IncreaseRate>();
            }

            var result = new List<IncreaseRate>();
            for (int i = 1; i < quotations.Count; i++)
            {
                result.Add(new IncreaseRate
                {
                    Date = quotations[i].Date,
                    Rate = quotations[i].Close / quotations[i - 1].Close - 1
                });
            }
            return result;
        }

        /// <summary>
        /// 获取当前时间节点的最新年报/半年报
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastAnnualReportDate()
        {
            var now = DateTime.Now;
            if (now.Month < 4)
            {
                return new DateTime(now.Year - 1, 6, 30);
            }
            else if (now.Month < 9)
            {
                return new DateTime(now.Year - 1, 12, 31);
            }
            else
            {
                return new DateTime(now.Year, 6, 30);
            }
        }

        /// <summary>
        /// 计算年化收益率
        /// </summary>
        /// <returns></returns>
        public double CalcYieldAnnual(List<FundNav> navs)
        {
            if (navs == null || navs.Count <= 1)
            {
                return 0;
            }

            var first = navs.OrderBy(x => x.Date).First();
            var last = navs.OrderBy(x => x.Date).Last();
            var p = last.AccUnitNav / first.AccUnitNav - 1; // 策略收益
            var n = navs.Count - 1; // 策略执行天数
            return Math.Pow(1 + (double)p, 250.0 / n) - 1;
        }

        /// <summary>
        /// 计算最大回撤
        /// </summary>
        /// <returns></returns>
        public decimal CalcMaxDrawdown()
        {

        }

        /// <summary>
        /// 计算波动率
        /// </summary>
        /// <returns></returns>
        public decimal CalcVolatility()
        {

        }

        /// <summary>
        /// 计算夏普率
        /// </summary>
        /// <returns></returns>
        public decimal CalcSharpe()
        {

        }
    }
}
