using HtmlAgilityPack;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
            return Math.Pow(1 + (double)p, 365.0 / n) - 1;
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
            var first = navs.OrderBy(x => x.Date).First();
            var last = navs.OrderBy(x => x.Date).Last();
            var n = (last.Date - first.Date).TotalDays - 1; // 策略执行天数
            return Math.Sqrt(rates.Sum(x => Math.Pow((double)x.Rate - average, 2)) / (n - 1) * 365);
        }

        /// <summary>
        /// 计算夏普率
        /// </summary>
        /// <returns></returns>
        public async Task<double> CalcSharpe(List<FundNav> navs, double? riskFreeRate = null)
        {
            var yieldAnnual = this.CalcYieldAnnual(navs);
            var volatility = this.CalcYieldAnnual(navs);
            if (riskFreeRate == null)
            {
                riskFreeRate = await this.GetRiskFreeRate();
            }
            return (yieldAnnual - riskFreeRate.Value) / volatility; // 无风险利率参考一年期定存利率
        }

        /// <summary>
        /// 计算下行波动率
        /// </summary>
        /// <param name="navs"></param>
        /// <returns></returns>
        public double CalcDownsideRisk(List<FundNav> navs)
        {
            var rates = this.GetIncreaseRates(navs); // 策略每日收益率
            var first = navs.OrderBy(x => x.Date).First();
            var last = navs.OrderBy(x => x.Date).Last();
            var n = (last.Date - first.Date).TotalDays - 1; // 策略执行天数
            var rpi = 0d; // 策略至第 i 日的平均收益率
            var risk = 0d;
            for (int i = 0; i < rates.Count; i++)
            {
                var rate = (double)rates[i].Rate;
                rpi = (rpi * i + rate) / (i + 1);
                if (rate < rpi)
                {
                    risk += Math.Pow(rate - rpi, 2);
                }
            }
            risk = Math.Sqrt(risk * 365 / n);
            return risk;
        }

        /// <summary>
        /// 计算索提诺比率
        /// </summary>
        /// <param name="navs"></param>
        /// <param name="riskFreeRate"></param>
        /// <returns></returns>
        public async Task<double> CalcSortinoRatio(List<FundNav> navs, double? riskFreeRate = null)
        {
            var yieldAnnual = this.CalcYieldAnnual(navs);
            if (riskFreeRate == null)
            {
                riskFreeRate = await this.GetRiskFreeRate();
            }
            var downsideRisk = this.CalcDownsideRisk(navs);
            return (yieldAnnual - riskFreeRate.Value) / downsideRisk;
        }

        /// <summary>
        /// 无风险利率参考一年期定存利率
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetRiskFreeRate()
        {
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load("https://www.boc.cn/fimarkets/lilv/fd31/");
                var node = doc.DocumentNode.SelectSingleNode("//div[@class='main']/div[@class='news']/ul/li/a");
                var url = $"https://www.boc.cn/fimarkets/lilv/fd31/{node.Attributes["href"].Value.Substring(2)}";
                using (var httpClient = new HttpClient())
                {
                    var html = await httpClient.GetStringAsync(url);
                    doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var nodes = doc.DocumentNode.SelectNodes("//div[@class='TRS_Editor']/table/tbody/tr");
                    foreach (var tr in nodes)
                    {
                        var tds = tr.SelectNodes("td");
                        if (tds == null)
                        {
                            continue;
                        }

                        if (tds[0].InnerText == "一年")
                        {
                            return double.Parse(WebUtility.HtmlDecode(tds[1].InnerText).Trim()) / 100;
                        }
                    }

                    return 0.0145;
                }
            }
            catch
            {
                return 0.0145;
            }
        }
    }
}
