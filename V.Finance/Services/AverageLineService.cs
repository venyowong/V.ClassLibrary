using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V.Finance.Models;

namespace V.Finance.Services
{
    /// <summary>
    /// 均线服务<br/>
    /// </summary>
    public class AverageLineService
    {
        private const int _minInterval = 20;
        private const int _maxInterval = 90;
        private const int _minContinuity = 2;
        private const int _maxContinuity = 10;

        private int continuity;
        private int interval;
        private List<Point> line;
        private readonly List<Point> points;

        public AverageLineService(List<Point> points)
        {
            this.points = points;
        }

        public (int Interval, int Continuity) CalcBestAverageLine()
        {
            int bestInterval = 0;
            int bestContinuity = 0;
            decimal bestScore = decimal.MinValue;
            for (int interval = _minInterval; interval <= _maxInterval; interval += 10)
            {
                for (int continuity = _minContinuity; continuity <= _maxContinuity; continuity++)
                {
                    decimal score = this.CalculateScore(interval, continuity);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestInterval = interval;
                        bestContinuity = continuity;
                    }
                }
            }
            return (bestInterval, bestContinuity);
        }

        /// <summary>
        /// 当前价格在均线之上，并且均线过去N个交易日处于上升趋势，则认为是上升趋势
        /// </summary>
        /// <param name="date"></param>
        /// <returns>-1 下降趋势 0 无明显趋势 1 上升趋势</returns>
        public int DetermineTrend(DateTime date)
        {
            var p = this.points.FirstOrDefault(x => x.Date == date);
            if (p == null)
            {
                return 0;
            }
            var l = this.line.FirstOrDefault(x => x.Date == date);
            if (l == null)
            {
                return 0;
            }

            var idx = this.line.IndexOf(l);
            if (p.Price > l.Price)
            {
                for (int i = 1; i <= this.continuity; i++)
                {
                    var index = idx - i;
                    if (index < 0)
                    {
                        return 0;
                    }

                    if (line[index].Price >= line[index + 1].Price)
                    {
                        return 0;
                    }
                }
                return 1;
            }
            else if (p.Price < l.Price)
            {
                for (int i = 1; i <= this.continuity; i++)
                {
                    var index = idx - i;
                    if (index < 0)
                    {
                        return 0;
                    }

                    if (line[index].Price <= line[index + 1].Price)
                    {
                        return 0;
                    }
                }
                return -1;
            }

            return 0;
        }

        public void Use(int interval, int continuity)
        {
            this.interval = interval;
            this.continuity = continuity; 
            var financialService = new FinancialService();
            this.line = financialService.GetAverageLine(this.points, interval); // 均线
        }

        private decimal CalculateScore(int interval, int continuity)
        {
            var financialService = new FinancialService();
            var points = financialService.GetAverageLine(this.points, interval); // 均线
            decimal score = 1;
            int up = 0, down = 0, index = -1;
            for (int i = 1; i < points.Count; i++)
            {
                {
                    if (points[i].Price > points[i - 1].Price)
                    {
                        up++;
                        down = 0;
                    }
                    else if (points[i].Price < points[i - 1].Price)
                    {
                        down++;
                        up = 0;
                    }
                    else
                    {
                        if (index < 0)
                        {
                            down++;
                            up = 0;
                        }
                        else
                        {
                            up++;
                            down = 0;
                        }
                    }

                    if (up >= continuity)
                    {
                        if (index < 0) // 买点
                        {
                            index = i;
                        }
                    }
                    else if (down >= continuity)
                    {
                        if (index > 0) // 卖点
                        {
                            score *= points[i].Price / points[index].Price;
                            index = -1;
                        }
                    }
                }
            }
            return score - 1;
        }
    }
}
