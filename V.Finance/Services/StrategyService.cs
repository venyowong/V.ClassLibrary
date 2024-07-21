using MathNet.Numerics;
using Serilog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.Finance.Models;

namespace V.Finance.Services
{
    public class StrategyService
    {
        /// <summary>
        /// 获取基金剔除行业收益后的超额收益
        /// </summary>
        /// <param name="fundCode"></param>
        /// <returns></returns>
        public async Task<double?> GetAlphaBasedOnIndustry(string fundCode)
        {
            if (string.IsNullOrWhiteSpace(fundCode))
            {
                Log.Warning("Strategy.GetAlphaBasedOnIndustry 指令 fundcode 不能为空");
                return null;
            }

            try
            {
                #region 基础条件校验
                var fundService = new FundService();
                var info = fundService.GetFundInfo(fundCode);
                if (info == null)
                {
                    Log.Warning($"无法获取到 {fundCode} 的基金信息");
                    return null;
                }
                if (!info.FundType.Contains("股票") && !info.FundType.Contains("混合"))
                {
                    Log.Warning($"{fundCode} 的类型为 {info.FundType} 无法计算该基金剔除行业收益后的超额收益");
                    return null;
                }
                var assetConfig = fundService.GetFundAssetConfigs(fundCode)?.OrderByDescending(x => x.Date).FirstOrDefault();
                if (assetConfig == null)
                {
                    Log.Warning($"无法获取到 {fundCode} 的持仓配置");
                    return null;
                }
                if (assetConfig.StockRatio < 0.6m)
                {
                    Log.Warning($"{fundCode} 的股票仓位占比为 {assetConfig.StockRatio.ToString("P")}，不适用于计算该基金剔除行业收益后的超额收益");
                    return null;
                }
                #endregion

                #region 基础数据获取
                var financialService = new FinancialService();
                var reportDate = financialService.GetLastAnnualReportDate();
                var positions = await fundService.GetFundStockPositions(fundCode, reportDate);
                if (positions == null || !positions.Any())
                {
                    Log.Warning($"未能获取到 {fundCode} 的股票持仓，因此无法计算该基金剔除行业收益后的超额收益");
                    return null;
                }
                Log.Information($"使用 {fundCode} {reportDate.ToString("yyyy-MM-dd")} 的持仓数据进行计算");

                var navs = await fundService.GetFundNavs(fundCode);
                if (navs == null || !navs.Any())
                {
                    Log.Warning($"未能获取到 {fundCode} 的净值数据，因此无法计算该基金剔除行业收益后的超额收益");
                    return null;
                }
                navs = navs.Where(x => x.Date >= DateTime.Now.AddMonths(-6).Date).ToList();
                var firstNavDate = navs.Min(x => x.Date);
                var indexService = new IndexService();
                var stockIndustryMap = await indexService.GetStockIndustryMap();
                var industryQuotationMap = await indexService.GetIndexQuotationsInTimeRange(firstNavDate, DateTime.Now);
                #endregion

                #region 计算行业收益
                var industryProfitMap = new Dictionary<DateTime, decimal>();
                while (firstNavDate <= DateTime.Now)
                {
                    var rate = 0m;
                    var validData = true;
                    foreach (var position in positions)
                    {
                        if (!stockIndustryMap.ContainsKey(position.StockCode))
                        {
                            continue;
                        }

                        var indexCode = stockIndustryMap[position.StockCode];
                        var quotation = industryQuotationMap[indexCode].FirstOrDefault(x => x.Date == firstNavDate);
                        if (quotation == null)
                        {
                            validData = false;
                            break;
                        }

                        rate += position.Ratio * quotation.Markup;
                    }
                    if (validData)
                    {
                        industryProfitMap.Add(firstNavDate, rate);
                    }
                    else
                    {
                        validData = true;
                    }

                    firstNavDate = firstNavDate.AddDays(1);
                }
                #endregion

                var xs = new List<double>();
                var ys = new List<double>();
                for (int i = 1; i < navs.Count; i++)
                {
                    if (!industryProfitMap.ContainsKey(navs[i].Date))
                    {
                        continue;
                    }

                    xs.Add((double)industryProfitMap[navs[i].Date]);
                    ys.Add((double)(navs[i].AccUnitNav / navs[i - 1].AccUnitNav - 1));
                }

                return Fit.Line(xs.ToArray(), ys.ToArray()).Item1;
            }
            catch (Exception e)
            {
                Log.Error(e, $"计算 {fundCode} 剔除行业收益后的超额收益时发生异常");
                return null;
            }
        }

        #region 择时能力
        public async Task<double?> GetFundTimingAbility(string fundCode)
        {
            #region 基础条件校验
            var fundService = new FundService();
            var info = fundService.GetFundInfo(fundCode);
            if (info == null)
            {
                Log.Warning($"无法获取到 {fundCode} 的基金信息");
                return null;
            }
            if (!info.FundType.Contains("股票") && !info.FundType.Contains("混合"))
            {
                Log.Warning($"{fundCode} 的类型为 {info.FundType} 无法计算该基金的择时能力");
                return null;
            }
            if (info.BeginDate > DateTime.Now.Date.AddYears(-2))
            {
                Log.Warning($"{fundCode} 成立未满两年，无法计算该基金的择时能力");
                return null;
            }
            var assetConfig = fundService.GetFundAssetConfigs(fundCode)?.OrderByDescending(x => x.Date).FirstOrDefault();
            if (assetConfig == null)
            {
                Log.Warning($"无法获取到 {fundCode} 的持仓配置");
                return null;
            }
            if (assetConfig.StockRatio < 0.6m)
            {
                Log.Warning($"{fundCode} 的股票仓位占比为 {assetConfig.StockRatio.ToString("P")}，不适用于计算该基金的择时能力");
                return null;
            }
            #endregion

            var begin = DateTime.Now.Date.AddYears(-2);
            var end = DateTime.Now.Date;
            var exposures = await GetFundStyleExposures2(fundCode, begin.AddMonths(-2), end);
            var inflections = (await new MarketService().GetStyleInflections())
                .FindAll(x => x.Date >= begin && x.Date <= end);
            var scores = new List<double>();
            for (int i = 0; i < inflections.Count; i++)
            {
                var nextInflection = inflections.FirstOrDefault(x => x.Date > inflections[i].Date && x.IndexCode == inflections[i].IndexCode);
                scores.Add(GetOperationScore(inflections[i], exposures, nextInflection != null ? nextInflection.Date : DateTime.Now));
            }
            if (!scores.Any())
            {
                return null;
            }

            return scores.Average();
        }

        private double GetOperationScore(StyleInflection inflection, List<FundStyleExposure> exposures, DateTime end)
        {
            double? r = null;
            double score = double.NaN;
            int keeping = 0, giveup = 0;
            foreach (var exposure in exposures.Where(x => x.Date >= inflection.Date.AddMonths(-2) && x.Date <= end))
            {
                var e = GetExposure(inflection.IndexCode, exposure);
                if (r == null)
                {
                    r = e;
                }
                if (double.IsNaN(score) && r != null && inflection.Type > 0 && ((r < 0 && e > 0) || (e - r) / Math.Abs(r.Value) >= 1.0 / 3))
                {
                    score = GetOperationScore(inflection.Date, exposure.Date);
                }
                if (double.IsNaN(score) && r != null && inflection.Type < 0 && ((r > 0 && e < 0) || (r - e) / Math.Abs(r.Value) >= 1.0 / 3))
                {
                    score = GetOperationScore(inflection.Date, exposure.Date);
                }

                if (exposure.Date <= end && exposure.Date >= inflection.Date)
                {
                    if ((inflection.Type > 0 && (e > 0 || e > r)) || (inflection.Type < 0 && (e < 0 || e < r)))
                    {
                        keeping++;
                    }
                    else
                    {
                        giveup++;
                    }
                }
            }

            if (double.IsNaN(score) || keeping + giveup == 0)
            {
                return 0;
            }
            return score * keeping / (keeping + giveup);
        }

        private double? GetExposure(string index, FundStyleExposure exposure)
        {
            switch (index)
            {
                case "CI005917":
                    return exposure.R1;
                case "CI005918":
                    return exposure.R2;
                case "CI005919":
                    return exposure.R3;
                case "CI005920":
                    return exposure.R4;
            }

            return null;
        }

        private double GetOperationScore(DateTime date1, DateTime date2)
        {
            if (date2 < date1)
            {
                return 3;
            }
            var days = (date2 - date1).TotalDays;
            if (days <= 30)
            {
                return 2.5;
            }
            if (days <= 90)
            {
                return 2;
            }
            if (days <= 180)
            {
                return 1.5;
            }
            if (days <= 270)
            {
                return 1;
            }
            if (days <= 365)
            {
                return 0.5;
            }
            return 0;
        }

        private async Task<List<FundStyleExposure>> GetFundStyleExposures2(string fundCode, DateTime begin, DateTime end)
        {
            var exposures = new List<FundStyleExposure>();
            while (begin <= end)
            {
                var exposure = await GetStyleExposure(fundCode, begin);
                if (exposure != null)
                {
                    exposures.Add(exposure);
                }
                begin = begin.AddDays(1);
            }
            return exposures;
        }

        private async Task<FundStyleExposure> GetStyleExposure(string fundCode, DateTime end)
        {
            var regression1 = await RollingRegression(fundCode, "CI005917", end);
            if (double.IsNaN(regression1.R2))
            {
                return null;
            }
            var regression2 = await RollingRegression(fundCode, "CI005918", end);
            if (double.IsNaN(regression2.R2))
            {
                return null;
            }
            var regression3 = await RollingRegression(fundCode, "CI005919", end);
            if (double.IsNaN(regression3.R2))
            {
                return null;
            }
            var regression4 = await RollingRegression(fundCode, "CI005920", end);
            if (double.IsNaN(regression4.R2))
            {
                return null;
            }
            var sum = regression1.R2 + regression2.R2 + regression3.R2 + regression4.R2;
            return new FundStyleExposure
            {
                FundCode = fundCode,
                Date = end,
                R1 = regression1.Beta / Math.Abs(regression1.Beta) * regression1.R2 / sum,
                R2 = regression2.Beta / Math.Abs(regression2.Beta) * regression2.R2 / sum,
                R3 = regression3.Beta / Math.Abs(regression3.Beta) * regression3.R2 / sum,
                R4 = regression4.Beta / Math.Abs(regression4.Beta) * regression4.R2 / sum
            };
        }

        private async Task<(double R2, double Beta)> RollingRegression(string fundCode, string index, DateTime end)
        {
            var fundExcessReturn = await GetFundExcessReturn(fundCode, end.AddDays(-13 * 7), end);
            if (!fundExcessReturn.Any())
            {
                return (double.NaN, 1);
            }

            var styleExcessReturn = await this.GetStyleExcessReturn(index, end.AddDays(-13 * 7), end);
            var y = new List<double>();
            var x = new List<double>();
            foreach (var item in styleExcessReturn)
            {
                var fundItem = fundExcessReturn.FirstOrDefault(it => it.Begin == item.Begin);
                if (fundItem == null)
                {
                    continue;
                }

                y.Add((double)fundItem.ExcessReturn);
                x.Add((double)item.ExcessReturn);
            }
            if (x.Count < 2)
            {
                return (double.NaN, 1);
            }

            var fitResult = Fit.Line(x.ToArray(), y.ToArray());
            var y2 = x.Select(item => fitResult.Item1 + fitResult.Item2 * item).ToArray();
            return (GoodnessOfFit.RSquared(y2, y), fitResult.Item2);
        }

        private async Task<List<dynamic>> GetFundExcessReturn(string fundCode, DateTime begin, DateTime end)
        {
            var indexQuotations = await new IndexService().GetIndexQuotations(begin, end, "000985");
            var navs = await new FundService().GetFundNavs(fundCode);
            var result = new List<dynamic>();

            while (begin <= end)
            {
                var quotations = indexQuotations.Where(x => x.Date >= begin && x.Date <= begin.AddDays(7)).ToList();
                if (!quotations.Any())
                {
                    begin = begin.AddDays(7);
                    continue;
                }

                var indexRate = quotations.Last().Close / quotations.First().Close - 1;
                var subNavs = navs.Where(x => x.Date >= begin && x.Date <= begin.AddDays(7)).ToList();
                if (!subNavs.Any())
                {
                    begin = begin.AddDays(7);
                    continue;
                }

                dynamic excessReturn = new ExpandoObject();
                excessReturn.Begin = begin.AddDays(1);
                excessReturn.End = begin.AddDays(7);
                excessReturn.ExcessReturn = subNavs.Last().AccUnitNav / subNavs[0].AccUnitNav - 1 - indexRate;
                result.Add(excessReturn);

                begin = begin.AddDays(7);
            }

            return result;
        }

        private async Task<List<dynamic>> GetStyleExcessReturn(string index, DateTime begin, DateTime end)
        {
            var indexService = new IndexService();
            var indexQuotations = await indexService.GetIndexQuotations(begin, end, "000985");
            var styleQuotations = await indexService.GetIndexQuotations(begin, end, index);
            var result = new List<dynamic>();

            while (begin <= end)
            {
                var quotations = indexQuotations.Where(x => x.Date >= begin && x.Date <= begin.AddDays(7)).ToList();
                if (!quotations.Any())
                {
                    begin = begin.AddDays(7);
                    continue;
                }

                var indexRate = quotations.Last().Close / quotations.First().Close - 1;
                var quotations2 = styleQuotations.Where(x => x.Date >= begin && x.Date <= begin.AddDays(7)).ToList();
                if (!quotations2.Any())
                {
                    break;
                }

                dynamic excessReturn = new ExpandoObject();
                excessReturn.Begin = begin.AddDays(1);
                excessReturn.End = begin.AddDays(7);
                excessReturn.ExcessReturn = quotations2.Last().Close / quotations2.First().Close - 1 - indexRate;
                result.Add(excessReturn);

                begin = begin.AddDays(7);
            }

            return result;
        }
        #endregion
    }
}
