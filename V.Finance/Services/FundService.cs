using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.Finance.Models;

namespace V.Finance.Services
{
    public class FundService
    {
        public async Task<List<FundNav>> GetFundNavs(string fundCode)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Referer", $"http://fundf10.eastmoney.com/jjjz_{fundCode}.html");
                    var response = await client.GetAsync($"http://api.fund.eastmoney.com/f10/lsjz?fundCode={fundCode}&pageIndex=1&pageSize=10000");
                    var json = await response.Content.ReadAsStringAsync();
                    var result = json.ToObj<JObject>();
                    return result["Data"]["LSJZList"].Select(x =>
                    {
                        if (!DateTime.TryParse(x["FSRQ"].ToString(), out DateTime date))
                        {
                            return default;
                        }
                        if (!decimal.TryParse(x["DWJZ"].ToString(), out decimal unitNav))
                        {
                            return default;
                        }
                        if (!decimal.TryParse(x["LJJZ"].ToString(), out decimal accUnitNav))
                        {
                            return default;
                        }
                        return new FundNav
                        {
                            FundCode = fundCode,
                            Date = date,
                            UnitNav = unitNav,
                            AccUnitNav = accUnitNav
                        };
                    })
                    .Where(x => x != null)
                    .OrderBy(x => x.Date)
                    .ToList();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"EmService.GetFundNavs {fundCode} 获取净值失败");
                return null;
            }
        }

        public List<FundAssetConfig> GetFundAssetConfigs(string fundCode)
        {
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load($"http://fundf10.eastmoney.com/zcpz_{fundCode}.html");
                var rows = doc.DocumentNode.SelectNodes("//table[@class='w782 comm tzxq']/tbody/tr");
                return rows.Select(x =>
                {
                    var columns = x.SelectNodes("td");
                    DateTime.TryParse(columns[0].InnerText, out DateTime date);
                    decimal.TryParse(columns[1].InnerText.Replace("%", ""), out decimal stock);
                    decimal.TryParse(columns[2].InnerText.Replace("%", ""), out decimal bond);
                    decimal.TryParse(columns[3].InnerText.Replace("%", ""), out decimal cash);
                    decimal.TryParse(columns[4].InnerText, out decimal net);
                    return new FundAssetConfig
                    {
                        Date = date,
                        NetAsset = net * 100000000,
                        BondRatio = bond / 100m,
                        CashRatio = cash / 100m,
                        FundCode = fundCode,
                        StockRatio = stock / 100m
                    };
                })
                .OrderBy(x => x.Date)
                .ToList();
            }
            catch (Exception e)
            {
                Log.Error(e, $"FundService.GetFundAssetConfigs {fundCode} 获取基金资产配置失败");
                return null;
            }
        }

        public async Task<List<FundScale>> GetFundScales(string fundCode)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync($"http://fundf10.eastmoney.com/FundArchivesDatas.aspx?type=gmbd&code={fundCode}");
                    var regex = new Regex("\"data\":(\\[[^\\]]*\\])");
                    var match = regex.Match(response);
                    if (!match.Success)
                    {
                        return null;
                    }

                    var array = match.Groups[1].Value.ToObj<JArray>();
                    return array.Select(x =>
                    {
                        DateTime.TryParse(x["FSRQ"]?.ToString(), out DateTime date);
                        decimal.TryParse(x["QJSG"]?.ToString(), out decimal purchase);
                        decimal.TryParse(x["QJSH"]?.ToString(), out decimal redeem);
                        decimal.TryParse(x["QMZFE"]?.ToString(), out decimal share);
                        decimal.TryParse(x["CHANGE"]?.ToString(), out decimal changeRate);
                        decimal.TryParse(x["NETNAV"]?.ToString(), out decimal netNav);
                        return new FundScale
                        {
                            ChangeRate = changeRate,
                            Date = date,
                            NetNav = netNav,
                            Purchase = purchase,
                            Redeem = redeem,
                            Share = share,
                            FundCode = fundCode
                        };
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"EmService.GetFundScales {fundCode} 获取基金份额数据失败");
                return null;
            }
        }

        public FundInfo GetFundInfo(string fundCode)
        {
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load($"http://fundf10.eastmoney.com/jbgk_{fundCode}.html");
                var rows = doc.DocumentNode.SelectNodes("//table[@class='info w790']/tr");
                var info = new FundInfo
                {
                    FundFullName = rows[0].SelectNodes("td")[0].InnerText,
                    FundName = rows[0].SelectNodes("td")[1].InnerText,
                    FundCode = fundCode,
                    FundType = rows[1].SelectNodes("td")[1].InnerText,
                    Administrator = rows[4].SelectNodes("td")[0].InnerText,
                    Trustee = rows[4].SelectNodes("td")[1].InnerText,
                    Manager = rows[5].SelectNodes("td")[0].InnerText,
                    Benchmark = rows.Count == 10 ? rows[9].SelectNodes("td")[0].InnerText : rows[8].SelectNodes("td")[0].InnerText,
                    Tracking = rows.Count == 10 ? rows[9].SelectNodes("td")[1].InnerText : rows[8].SelectNodes("td")[1].InnerText,
                    IssueDate = rows[2].SelectNodes("td")[0].InnerText.TryParseDateTime("yyyy年MM月dd日")
                };
                var dateRegex = new Regex("\\d{4}年\\d{2}月\\d{2}日");
                var match = dateRegex.Match(rows[2].SelectNodes("td")[1].InnerText);
                if (match.Success)
                {
                    info.BeginDate = match.Value.TryParseDateTime("yyyy年MM月dd日");
                }
                var numRegex = new Regex("(\\d+.?\\d*)亿");
                match = numRegex.Match(rows[3].SelectNodes("td")[0].InnerText);
                if (match.Success)
                {
                    info.Scale = decimal.Parse(match.Groups[1].Value) * 100000000;
                }
                match = numRegex.Match(rows[3].SelectNodes("td")[1].InnerText);
                if (match.Success)
                {
                    info.Share = decimal.Parse(match.Groups[1].Value) * 100000000;
                }
                var numRegex2 = new Regex("(\\d+.?\\d*)元");
                match = numRegex2.Match(rows[5].SelectNodes("td")[1].InnerText);
                if (match.Success)
                {
                    info.Bonus = decimal.Parse(match.Groups[1].Value);
                }
                var percentRegex = new Regex("(\\d+.?\\d*)%");
                match = percentRegex.Match(rows[6].SelectNodes("td")[0].InnerText);
                if (match.Success)
                {
                    info.ManagementRate = decimal.Parse(match.Groups[1].Value) / 100;
                }
                match = percentRegex.Match(rows[6].SelectNodes("td")[1].InnerText);
                if (match.Success)
                {
                    info.EscrowRate = decimal.Parse(match.Groups[1].Value) / 100;
                }
                match = percentRegex.Match(rows[7].SelectNodes("td")[0].InnerText);
                if (match.Success)
                {
                    info.SaleServiceRate = decimal.Parse(match.Groups[1].Value) / 100;
                }
                match = percentRegex.Match(rows[7].SelectNodes("td")[1].InnerText);
                if (match.Success)
                {
                    info.MaxSubscriptionRate = decimal.Parse(match.Groups[1].Value) / 100;
                }
                if (rows.Count == 9)
                {
                    var tds = doc.DocumentNode.SelectNodes("//table[@class='info w790']/td");
                    match = percentRegex.Match(tds[0].SelectNodes("span")[0].InnerText);
                    if (match.Success)
                    {
                        info.MaxPurchaseRate = decimal.Parse(match.Groups[1].Value) / 100;
                    }
                    match = percentRegex.Match(tds[1].InnerText);
                    if (match.Success)
                    {
                        info.MaxRedeemRate = decimal.Parse(match.Groups[1].Value) / 100;
                    }
                }
                else
                {
                    match = percentRegex.Match(rows[8].SelectNodes("td")[0].InnerText);
                    if (match.Success)
                    {
                        info.MaxPurchaseRate = decimal.Parse(match.Groups[1].Value) / 100;
                    }
                    match = percentRegex.Match(rows[8].SelectNodes("td")[1].InnerText);
                    if (match.Success)
                    {
                        info.MaxRedeemRate = decimal.Parse(match.Groups[1].Value) / 100;
                    }
                }
                var boxes = doc.DocumentNode.SelectNodes("//div[@class='boxitem w790']");
                info.InvestmentTarget = boxes[0].SelectSingleNode("p").InnerText.Trim();
                info.InvestmentPhilosophy = boxes[1].SelectSingleNode("p").InnerText.Trim();
                info.InvestmentScope = boxes[2].SelectSingleNode("p").InnerText.Trim();
                info.InvestmentStrategy = boxes[3].SelectSingleNode("p").InnerText.Trim();
                info.DividendPolicy = boxes[4].SelectSingleNode("p").InnerText.Trim();
                info.Risk = boxes[5].SelectSingleNode("p").InnerText.Trim();
                return info;
            }
            catch (Exception e)
            {
                Log.Error(e, $"EmService.GetFundInfo {fundCode} 获取基金信息失败");
                return null;
            }
        }

        public async Task<List<FundStockPosition>> GetFundStockPositions(string fundCode, DateTime date)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync($"http://fundf10.eastmoney.com/FundArchivesDatas.aspx?type=jjcc&code={fundCode}&year={date.Year}&month={date.Month}");
                    var regex = new Regex("content:\"([^\"]*)\"");
                    var match = regex.Match(response);
                    if (!match.Success)
                    {
                        return null;
                    }

                    var doc = new HtmlDocument();
                    doc.LoadHtml(match.Groups[1].Value);
                    var rows = doc.DocumentNode.SelectNodes("//tbody/tr");
                    return rows?.Select(r =>
                    {
                        var columns = r.SelectNodes("td");
                        if (columns == null || !columns.Any())
                        {
                            return null;
                        }

                        var position = new FundStockPosition
                        {
                            StockCode = columns[1].InnerText,
                            StockName = columns[2].InnerText,
                            ReportDate = date,
                            FundCode = fundCode
                        };
                        if (columns.Count == 9)
                        {
                            decimal.TryParse(columns[6].InnerText.Replace("%", ""), out decimal ratio);
                            decimal.TryParse(columns[7].InnerText.Replace(",", ""), out decimal share);
                            decimal.TryParse(columns[8].InnerText.Replace(",", ""), out decimal marketValue);
                            position.Ratio = ratio / 100m;
                            position.Share = share * 10000;
                            position.MarketValue = marketValue * 10000;
                        }
                        else
                        {
                            decimal.TryParse(columns[4].InnerText.Replace("%", ""), out decimal ratio);
                            decimal.TryParse(columns[5].InnerText.Replace(",", ""), out decimal share);
                            decimal.TryParse(columns[6].InnerText.Replace(",", ""), out decimal marketValue);
                            position.Ratio = ratio / 100m;
                            position.Share = share * 10000;
                            position.MarketValue = marketValue * 10000;
                        }
                        return position;
                    })
                    .Where(x => x != null)
                    .ToList();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"EmService.GetFundStockPositions {fundCode} {date} 获取基金股票持仓失败");
                return null;
            }
        }

        private static readonly Regex _ranksRegex = new Regex("\\{[^\\}]*\\\"");
        /// <summary>
        /// 获取基金排行
        /// </summary>
        /// <param name="fundType">基金类型 hh 混合 gp 股票 qdii zq 债券</param>
        /// <param name="page">页数，从 1 开始</param>
        /// <param name="size">每页数量</param>
        /// <param name="subType">041 长期纯债 042 短期纯债</param>
        /// <returns></returns>
        public async Task<List<FundRankModel>> GetFundRanks(string fundType, int page, int size, string subType = null)
        {
            // http://fund.eastmoney.com/data/rankhandler.aspx?op=ph&dt=kf&ft=hh&gs=0&sc=1nzf&st=desc&sd=2019-11-12&ed=2020-11-18&pi=1&pn=50&dx=0
            // sc：1nzf 一年涨幅  sd：起始日期  ed：截止日期  pn：数量  dx：0 全部 1 可购
            var url = $"http://fund.eastmoney.com/data/rankhandler.aspx?op=ph&dt=kf&ft={fundType}&gs=0&sc=1nzf&st=desc&pi={page}&pn={size}&dx=0&qdii={subType}|&tabSubtype={subType},,,,,";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Referer", "https://fund.eastmoney.com/data/fundranking.html");
                var text = await client.GetStringAsync(url);
                text = _ranksRegex.Match(text).Groups[0].Value;
                var json = text.ToObj<JObject>();
                var result = new List<FundRankModel>();
                foreach (var item in json["datas"]) 
                {
                    try
                    {
                        var strs = item.ToString().Split(',');
                        result.Add(new FundRankModel
                        {
                            FundCode = strs[0],
                            FundName = strs[1],
                            Date = DateTime.Parse(strs[3]),
                            YieldYear1 = decimal.Parse(strs[11])
                        });
                    }
                    catch { }
                }
                return result;
            }
        }

        /// <summary>
        /// 获取基金持有人结构
        /// </summary>
        /// <param name="fundCode"></param>
        /// <returns></returns>
        public List<FundHolderStructure> GetFundHolderStructures(string fundCode)
        {
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load($"https://fundf10.eastmoney.com/FundArchivesDatas.aspx?type=cyrjg&code={fundCode}");
                var rows = doc.DocumentNode.SelectNodes("//table[@class='w782 comm cyrjg']/tbody/tr");
                return rows.Select(x =>
                {
                    var columns = x.SelectNodes("td");
                    DateTime.TryParse(columns[0].InnerText, out DateTime date);
                    decimal.TryParse(columns[1].InnerText.Replace("%", ""), out decimal org);
                    decimal.TryParse(columns[2].InnerText.Replace("%", ""), out decimal personal);
                    decimal.TryParse(columns[3].InnerText.Replace("%", ""), out decimal inner);
                    decimal.TryParse(columns[4].InnerText, out decimal share);
                    return new FundHolderStructure
                    {
                        Date = date,
                        Share = share * 100000000,
                        OrgRatio = org / 100m,
                        PersonalRatio = personal / 100m,
                        FundCode = fundCode,
                        InnerRatio = inner / 100m
                    };
                })
                .OrderBy(x => x.Date)
                .ToList();
            }
            catch (Exception e)
            {
                Log.Error(e, $"FundService.GetFundHolderStructures {fundCode} 获取基金持有人结构失败");
                return null;
            }
        }
    }
}
