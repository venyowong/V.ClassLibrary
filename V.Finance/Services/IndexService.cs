using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.Finance.Models;

namespace V.Finance.Services
{
    public class IndexService
    {
        public static Dictionary<string, string> IndustryMap = new Dictionary<string, string>
        {
            { "801010", "农林牧渔" },
            { "801020", "采掘" },
            { "801030", "化工" },
            { "801040", "钢铁" },
            { "801050", "有色金属" },
            { "801080", "电子元器件" },
            { "801110", "家用电器" },
            { "801120", "食品饮料" },
            { "801130", "纺织服装" },
            { "801140", "轻工制造" },
            { "801150", "医药生物" },
            { "801160", "公用事业" },
            { "801170", "交通运输" },
            { "801180", "房地产" },
            { "801200", "商业贸易" },
            { "801210", "餐饮旅游" },
            { "801230", "综合" },
            { "801710", "建筑材料" },
            { "801720", "建筑装饰" },
            { "801730", "电气设备" },
            { "801740", "国防军工" },
            { "801750", "计算机" },
            { "801760", "传媒" },
            { "801770", "通信" },
            { "801780", "银行" },
            { "801790", "非银金融" },
            { "801880", "汽车" },
            { "801890", "机械设备" }
        };

        public async Task<List<IndexQuotation>> GetIndexQuotations(DateTime begin, DateTime end, params string[] codes)
        {
            using (var client = new HttpClient())
            {
                var indexCode = string.Join("','", codes);
                var url = $"http://www.swsindex.com/handler.aspx?tablename=swindexhistory&key=id&p={{0}}&where=swindexcode in ('{indexCode}') and  " +
                    $"BargainDate>='{begin.ToString("yyyy-MM-dd")}' and  BargainDate<='{end.ToString("yyyy-MM-dd")}'&orderby=swindexcode asc,BargainDate_1&" +
                    $"fieldlist=SwIndexCode,SwIndexName,BargainDate,CloseIndex,BargainAmount,Markup,OpenIndex,MaxIndex,MinIndex,BargainSum&pagecount=13664";
                var page = 1;
                var result = new List<IndexQuotation>();
                while (true)
                {
                    Log.Information($"获取申万指数行情数据('{indexCode}') {begin}~{end} 第{page}页数据...");
                    var json = await client.GetStringAsync(string.Format(url, page));
                    var data = json?.Replace('\'', '"').ToObj<JObject>()?["root"];
                    if (data == null || !data.Any())
                    {
                        return result;
                    }

                    foreach (var item in data)
                    {
                        var quotation = new IndexQuotation
                        {
                            IndexCode = item["SwIndexCode"].ToString(),
                            IndexName = item["SwIndexName"].ToString()
                        };
                        DateTime.TryParse(item["BargainDate"].ToString(), out DateTime date);
                        decimal.TryParse(item["CloseIndex"].ToString(), out decimal close);
                        decimal.TryParse(item["BargainAmount"].ToString(), out decimal vol);
                        decimal.TryParse(item["Markup"].ToString(), out decimal markup);
                        decimal.TryParse(item["OpenIndex"].ToString(), out decimal open);
                        decimal.TryParse(item["MaxIndex"].ToString(), out decimal max);
                        decimal.TryParse(item["MinIndex"].ToString(), out decimal min);
                        decimal.TryParse(item["BargainSum"].ToString(), out decimal amount);
                        quotation.Date = date;
                        quotation.Close = close;
                        quotation.Volume = vol * 10000;
                        quotation.Markup = markup / 100;
                        quotation.Open = open;
                        quotation.Max = max;
                        quotation.Min = min;
                        quotation.Amount = amount;
                        result.Add(quotation);
                    }

                    page++;
                }
            }
        }

        public async Task<List<IndexConstituent>> GetIndexConstituents(string code)
        {
            using (var client = new HttpClient())
            {
                var url = $"http://www.swsindex.com/handler.aspx?tablename=SwIndexConstituents&key=id&p={{0}}&where=SwIndexCode='{code}'  and " +
                    $"IsReserve ='0' and    NewFlag=1&orderby=StockCode ,BeginningDate_0&fieldlist=stockcode,stockname,newweight,beginningdate&pagecount=100";
                var page = 1;
                var result = new List<IndexConstituent>();
                while (true)
                {
                    Log.Information($"获取申万指数成分数据({code}) 第{page}页数据...");
                    var json = await client.GetStringAsync(string.Format(url, page));
                    var data = json?.Replace('\'', '"').ToObj<JObject>()?["root"];
                    if (data == null || !data.Any())
                    {
                        return result;
                    }

                    foreach (var item in data)
                    {
                        var constituent = new IndexConstituent
                        {
                            IndexCode = code,
                            StockCode = item["stockcode"].ToString(),
                            StockName = item["stockname"].ToString()
                        };
                        decimal.TryParse(item["newweight"].ToString(), out decimal weight);
                        DateTime.TryParse(item["beginningdate"].ToString(), out DateTime begin);
                        constituent.Weight = weight;
                        constituent.Begin = begin;
                        result.Add(constituent);
                    }

                    page++;
                }
            }
        }

        public async Task<Dictionary<string, string>> GetStockIndustryMap()
        {
            var stockIndustryMap = new Dictionary<string, string>();
            foreach (var indexCode in IndustryMap.Keys)
            {
                var constituents = await this.GetIndexConstituents(indexCode);
                constituents?.ForEach(x => stockIndustryMap.Add(x.StockCode, indexCode));
            }
            return stockIndustryMap;
        }

        public async Task<Dictionary<string, List<IndexQuotation>>> GetIndexQuotationsInTimeRange(DateTime begin, DateTime end)
        {
            var result = new Dictionary<string, List<IndexQuotation>>();
            foreach (var indexCode in IndustryMap.Keys)
            {
                result.Add(indexCode, await this.GetIndexQuotations(begin, end, indexCode));
            }
            return result;
        }
    }
}
