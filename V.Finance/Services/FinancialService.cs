using HtmlAgilityPack;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
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
            return this.GetIncreaseRates(quotations.Select(x => new Point
            {
                Date = x.Date,
                Price = x.Close
            }).ToList());
        }

        public List<IncreaseRate> GetIncreaseRates(List<FundNav> navs)
        {
            return this.GetIncreaseRates(navs.Select(x => new Point
            {
                Date = x.Date,
                Price = x.AccUnitNav
            }).ToList());
        }

        public List<IncreaseRate> GetIncreaseRates(List<Point> points)
        {
            points = points.OrderBy(x => x.Date).ToList();
            var result = new List<IncreaseRate>();
            for (int i = 1; i < points.Count; i++)
            {
                result.Add(new IncreaseRate
                {
                    Date = points[i].Date,
                    Rate = points[i].Price / points[i - 1].Price - 1
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

            return this.CalcYieldAnnual(navs.Select(x => new Point
            {
                Date = x.Date,
                Price = x.AccUnitNav
            }).ToList());
        }

        /// <summary>
        /// 计算年化收益率
        /// </summary>
        /// <returns>年化收益率(小数形式)</returns>
        public double CalcYieldAnnual(List<Point> points)
        {
            if (points == null || points.Count <= 1)
            {
                return 0;
            }

            points = points.OrderBy(x => x.Date).ToList();
            var first = points.OrderBy(x => x.Date).First();
            var last = points.OrderBy(x => x.Date).Last();
            var p = last.Price / first.Price - 1; // 策略收益
            var n = (last.Date - first.Date).TotalDays; // 策略执行天数
            return Math.Pow(1 + (double)p, 365.0 / n) - 1;
        }

        /// <summary>
        /// 计算最大回撤
        /// </summary>
        /// <returns>返回最大回撤(小数形式)，若结果为 0 则表示没有发生回撤</returns>
        public double CalcMaxDrawdown(List<FundNav> navs)
        {
            return this.CalcMaxDrawdown(navs.Select(x => new Point
            {
                Date = x.Date,
                Price = x.AccUnitNav
            }).ToList());
        }

        /// <summary>
        /// 计算最大回撤
        /// </summary>
        /// <returns>返回最大回撤(小数形式)，若结果为 0 则表示没有发生回撤</returns>
        public double CalcMaxDrawdown(List<Point> points)
        {
            var rates = this.GetIncreaseRates(points);
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
            return (double)max;
        }

        /// <summary>
        /// 计算波动率
        /// </summary>
        /// <returns></returns>
        public double CalcVolatility(List<FundNav> navs)
        {
            return this.CalcVolatility(navs.Select(x => new Point
            {
                Date = x.Date,
                Price = x.AccUnitNav
            }).ToList());
        }

        /// <summary>
        /// 计算波动率
        /// </summary>
        /// <returns></returns>
        public double CalcVolatility(List<Point> points)
        {
            var rates = this.GetIncreaseRates(points); // 策略每日收益率
            var average = (double)rates.Average(x => x.Rate); // 策略每日收益率的平均值
            var first = points.OrderBy(x => x.Date).First();
            var last = points.OrderBy(x => x.Date).Last();
            var n = (last.Date - first.Date).TotalDays; // 策略执行天数
            return Math.Sqrt(rates.Sum(x => Math.Pow((double)x.Rate - average, 2)) / (n - 1) * 365);
        }

        /// <summary>
        /// 计算夏普率
        /// </summary>
        /// <returns></returns>
        public Task<double> CalcSharpe(List<FundNav> navs, double? riskFreeRate = null)
        {
            return this.CalcSharpe(navs.Select(x => new Point
            {
                Date = x.Date,
                Price = x.AccUnitNav
            }).ToList(), riskFreeRate);
        }

        /// <summary>
        /// 计算夏普率
        /// </summary>
        /// <returns></returns>
        public async Task<double> CalcSharpe(List<Point> points, double? riskFreeRate = null)
        {
            var yieldAnnual = this.CalcYieldAnnual(points);
            var volatility = this.CalcYieldAnnual(points);
            if (riskFreeRate == null)
            {
                riskFreeRate = await this.GetRiskFreeRate();
            }
            return (yieldAnnual - riskFreeRate.Value) / volatility; // 无风险利率参考一年期定存利率
        }

        /// <summary>
        /// 计算下行波动率
        /// </summary>
        /// <returns></returns>
        public double CalcDownsideRisk(List<FundNav> navs)
        {
            return this.CalcDownsideRisk(navs.Select(x => new Point
            {
                Date = x.Date,
                Price = x.AccUnitNav
            }).ToList());
        }

        /// <summary>
        /// 计算下行波动率
        /// </summary>
        /// <returns></returns>
        public double CalcDownsideRisk(List<Point> points)
        {
            var rates = this.GetIncreaseRates(points); // 策略每日收益率
            var first = points.OrderBy(x => x.Date).First();
            var last = points.OrderBy(x => x.Date).Last();
            var n = (last.Date - first.Date).TotalDays; // 策略执行天数
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
        /// <returns></returns>
        public Task<double> CalcSortinoRatio(List<FundNav> navs, double? riskFreeRate = null)
        {
            return this.CalcSortinoRatio(navs.Select(x => new Point
            {
                Date = x.Date,
                Price = x.AccUnitNav
            }).ToList(), riskFreeRate);
        }

        /// <summary>
        /// 计算索提诺比率
        /// </summary>
        /// <returns></returns>
        public async Task<double> CalcSortinoRatio(List<Point> points, double? riskFreeRate = null)
        {
            var yieldAnnual = this.CalcYieldAnnual(points);
            if (riskFreeRate == null)
            {
                riskFreeRate = await this.GetRiskFreeRate();
            }
            var downsideRisk = this.CalcDownsideRisk(points);
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

        public double CalcBeta(List<Point> points, List<Point> bases)
        {
            var alignLists = this.Align(points, bases);
            var sample1 = this.GetIncreaseRates(alignLists.List1).Select(x => (double)x.Rate).ToArray();
            var sample2 = this.GetIncreaseRates(alignLists.List2).Select(x => (double)x.Rate).ToArray();
            var cov = ArrayStatistics.Covariance(sample1, sample2);
            var variance = ArrayStatistics.Variance(sample2);
            return cov / variance;
        }

        public async Task<double> CalcAlpha(List<Point> points, List<Point> bases, double? riskFreeRate = null)
        {
            var alignLists = this.Align(points, bases);
            var rp = this.CalcYieldAnnual(alignLists.List1); // 策略年化收益率
            var rm = this.CalcYieldAnnual(alignLists.List2); // 基准年化收益率
            if (riskFreeRate == null)
            {
                riskFreeRate = await this.GetRiskFreeRate();
            }
            var beta = this.CalcBeta(points, bases);
            return rp - riskFreeRate.Value - beta * (rm - riskFreeRate.Value);
        }

        private (List<Point> List1, List<Point> List2) Align(List<Point> list1, List<Point> list2)
        {
            int i = 0, j = 0;
            DateTime date1 = DateTime.MinValue, date2 = DateTime.MinValue;
            (List<Point> List1, List<Point> List2) result = (new List<Point>(), new List<Point>());
            for (; i < list1.Count;)
            {
                date1 = list1[i].Date;
                if (date1 == date2)
                {
                    result.List1.Add(list1[i]);
                    result.List2.Add(list2[j]);
                    i++;
                    j++;
                }
                else if (date1 < date2)
                {
                    i++;
                }
                else
                {
                    for (; j < list2.Count;)
                    {
                        date2 = list2[j].Date;
                        if (date2 < date1)
                        {
                            j++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (j >= list2.Count)
                    {
                        break;
                    }
                }
            }

            return result;
        }
    }
}
