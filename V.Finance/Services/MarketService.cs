using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.Finance.Models;

namespace V.Finance.Services
{
    public class MarketService
    {
        #region 风格拐点
        public async Task<List<StyleInflection>> GetStyleInflections()
        {
            var result = new List<StyleInflection>();
            var indecies = new string[] { "CI005917", "CI005918", "CI005919", "CI005920" };
            foreach (var i in indecies)
            {
                var inflections = await this.GetStyleInflections(i);
                var inflection = inflections.OrderByDescending(x => x.Date).FirstOrDefault();
                if (inflection != null)
                {
                    result.Add(inflection);
                }
            }
            return result;
        }

        private async Task<List<StyleInflection>> GetStyleInflections(string index)
        {
            var result = new List<StyleInflection>();
            var quotations = await new IndexService().GetIndexQuotations(DateTime.Now.Date.AddYears(-5), DateTime.Now.Date, index);
            if (quotations.IsNullOrEmpty())
            {
                Log.Warning($"获取不到近五年的 {index} 行情数据，无法计算拐点");
                return result;
            }
            var lastDate = quotations.Last().Date;
            if (lastDate < DateTime.Now.AddMonths(-1).Date)
            {
                Log.Warning($"{index} 最新的行情日期为 {lastDate.ToString("yyyy-MM-dd")}，请及时更新数据");
            }
            else
            {
                Log.Information($"{index} 最新的行情日期为 {lastDate.ToString("yyyy-MM-dd")}");
            }

            var rates = new FinancialService().GetIncreaseRates(quotations);
            var inflection = this.GetInflection(rates, 0);
            while (inflection.Begin > DateTime.MinValue)
            {
                var q = quotations.FirstOrDefault(x => x.Date == inflection.Begin);
                result.Add(new StyleInflection
                {
                    Date = inflection.Begin,
                    Close = q?.Close ?? 0,
                    Type = inflection.Type,
                    IndexCode = q?.IndexCode,
                    IndexName = q?.IndexName
                });

                inflection = this.GetInflection(rates.FindAll(x => x.Date > inflection.End), -inflection.Type);
            }
            return result;
        }

        private (DateTime Begin, DateTime End, int Type) GetInflection(List<IncreaseRate> rates, int type)
        {
            if (type > 0)
            {
                var inflection = GetUpInflection(rates);
                return (inflection.Begin, inflection.End, 1);
            }
            if (type < 0)
            {
                var inflection = GetDownInflection(rates);
                return (inflection.Begin, inflection.End, -1);
            }

            var up = GetUpInflection(rates);
            var down = GetDownInflection(rates);
            if (up.Begin < down.Begin)
            {
                return (up.Begin, up.End, 1);
            }
            else
            {
                return (down.Begin, down.End, -1);
            }
        }

        private (DateTime Begin, DateTime End) GetUpInflection(List<IncreaseRate> rates)
        {
            var begin = DateTime.MinValue;
            var drawdown = decimal.MinValue;
            foreach (var rate in rates)
            {
                if (drawdown < 1)
                {
                    drawdown = rate.Rate + 1;
                    begin = rate.Date;
                }
                else
                {
                    drawdown *= rate.Rate + 1;
                }
                if (drawdown >= 1.15m && (rate.Date - begin).TotalDays >= 90)
                {
                    return (begin, rate.Date);
                }
            }

            return (DateTime.MinValue, DateTime.MaxValue);
        }

        private (DateTime Begin, DateTime End) GetDownInflection(List<IncreaseRate> rates)
        {
            var begin = DateTime.MinValue;
            var drawdown = decimal.MaxValue;
            foreach (var rate in rates)
            {
                if (drawdown > 1)
                {
                    drawdown = rate.Rate + 1;
                    begin = rate.Date;
                }
                else
                {
                    drawdown *= rate.Rate + 1;
                }
                if (drawdown <= 0.85m && (rate.Date - begin).TotalDays >= 90)
                {
                    return (begin, rate.Date);
                }
            }

            return (DateTime.MinValue, DateTime.MaxValue);
        }
        #endregion
    }
}
