using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.Finance.Models;

namespace V.Finance.Services
{
    public class StockService
    {
        /// <summary>
        /// 获取行情数据
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="type">0 股票 1 ETF 2 指数</param>
        /// <returns></returns>
        public async Task<List<StockPrice>> GetStockPrices(string stockCode, int type = 0)
        {
            try
            {
                if (type == 1 || stockCode.StartsWith("sh") || stockCode.StartsWith("sz"))
                {
                    return await GetETFPrices(stockCode);
                }

                var list = new List<StockPrice>();
                var code = type == 2 ? $"1.{stockCode}" : $"0.{stockCode}";
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"http://push2his.eastmoney.com/api/qt/stock/kline/get?fields1=f1,f2,f3,f4,f5,f6,f7,f8,f9,f10,f11,f12,f13&fields2=f51,f52,f53,f54,f55,f56,f57,f58,f59,f60,f61&beg=0&end=29990101&secid={code}&klt=101&fqt=1");
                    var json = await response.Content.ReadAsStringAsync();
                    var result = json.ToObj<JObject>();
                    foreach (var row in result["data"]["klines"])
                    {
                        var strs = row.ToString().Split(',');
                        list.Add(new StockPrice
                        {
                            StockCode = stockCode,
                            Date = DateTime.Parse(strs[0]),
                            Open = decimal.Parse(strs[1]),
                            Close = decimal.Parse(strs[2]),
                            Max = decimal.Parse(strs[3]),
                            Min = decimal.Parse(strs[4])
                        });
                    }
                    return list;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"EmService.GetStockPrices {stockCode} 失败");
                return null;
            }
        }

        public static async Task<StockPrice> GetETFPrice(string stockCode)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var json = await client.GetStringAsync($"http://web.juhe.cn/finance/stock/hs?key=36217050ea5782478adcf8de7e460dbc&gid={stockCode}");
                    var result = json.ToObj<JObject>();
                    var data = result["result"][0]["data"];
                    return new StockPrice
                    {
                        StockCode = stockCode,
                        Date = DateTime.Parse(data["date"].ToString()),
                        Open = decimal.Parse(data["todayStartPri"].ToString()),
                        Close = decimal.Parse(data["nowPri"].ToString()),
                        Max = decimal.Parse(data["todayMax"].ToString()),
                        Min = decimal.Parse(data["todayMin"].ToString())
                    };
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"EmService.GetETFPrice {stockCode} 失败");
                return null;
            }
        }

        public static async Task<List<StockPrice>> GetETFPrices(string stockCode)
        {
            if (stockCode.StartsWith("sh") || stockCode.StartsWith("sz"))
            {
                stockCode = stockCode.Substring(2);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var json = await client.GetStringAsync($"http://115.190.9.197:8080/api/public/fund_etf_hist_em?symbol={stockCode}&adjust=qfq");
                    var result = json.ToObj<JArray>();
                    var list = new List<StockPrice>();
                    foreach (var item in result)
                    {
                        list.Add(new StockPrice
                        {
                            StockCode = stockCode,
                            Date = DateTime.Parse(item["日期"].ToString()),
                            Open = decimal.Parse(item["开盘"].ToString()),
                            Close = decimal.Parse(item["收盘"].ToString()),
                            Max = decimal.Parse(item["最高"].ToString()),
                            Min = decimal.Parse(item["最低"].ToString())
                        });
                    }
                    return list;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"EmService.GetETFPrices {stockCode} 失败");
                return null;
            }
        }
    }
}
