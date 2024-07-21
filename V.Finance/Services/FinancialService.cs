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

        public List<IncreaseRate> GetIncreaseRates(List<FundNav> navs)
        {
            if (navs.IsNullOrEmpty())
            {
                return new List<IncreaseRate>();
            }

            navs = navs.OrderBy(x => x.Date).ToList();
            var result = new List<IncreaseRate>();
            for (int i = 1; i < navs.Count; i++)
            {
                result.Add(new IncreaseRate
                {
                    Date = navs[i].Date,
                    Rate = navs[i].AccUnitNav / navs[i - 1].AccUnitNav - 1
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
        /// <returns>年化收益率(小数形式)</returns>
        public double CalcYieldAnnual(List<FundNav> navs)
        {
            if (navs == null || navs.Count <= 1)
            {
                return 0;
            }

            navs = navs.OrderBy(x => x.Date).ToList();
            var first = navs.OrderBy(x => x.Date).First();
            var last = navs.OrderBy(x => x.Date).Last();
            var p = last.AccUnitNav / first.AccUnitNav - 1; // 策略收益
            var n = (last.Date - first.Date).TotalDays - 1; // 策略执行天数
            return Math.Pow(1 + (double)p, 250.0 / n) - 1;
        }

        /// <summary>
        /// 计算最大回撤
        /// </summary>
        /// <returns>返回最大回撤(小数形式)，若结果为 0 则表示没有发生回撤</returns>
        public decimal CalcMaxDrawdown(List<FundNav> navs)
        {
            var rates = this.GetIncreaseRates(navs);
            decimal p = 1, max = 0;
            foreach (var rate in rates)
            {
                p *= 1 + rate.Rate;
                if (p - 1 < max)
                {
                    max = p - 1;
                }
                if (p > 1)
                {
                    p = 1;
                }
            }
            return max;
        }

        /// <summary>
        /// 计算波动率
        /// </summary>
        /// <returns></returns>
        public double CalcVolatility(List<FundNav> navs)
        {
            var rates = this.GetIncreaseRates(navs); // 策略每日收益率
            var average = (double)rates.Average(x => x.Rate); // 策略每日收益率的平均值
            return Math.Sqrt(rates.Sum(x => Math.Pow((double)x.Rate - average, 2)) / (rates.Count - 1) * 250);
        }

        /// <summary>
        /// 计算夏普率
        /// </summary>
        /// <returns></returns>
        public double CalcSharpe(List<FundNav> navs)
        {
            var yieldAnnual = this.CalcYieldAnnual(navs);
            var volatility = this.CalcYieldAnnual(navs);
            return (yieldAnnual - 0.0145) / volatility; // 无风险利率参考一年期定存利率
        }
    }
}
