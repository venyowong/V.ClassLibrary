using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class StyleInflection
    {
        public string IndexCode { get; set; }

        public string IndexName { get; set; }

        public DateTime Date { get; set; }

        public decimal Close { get; set; }

        /// <summary>
        /// 1 上行 -1 下行
        /// </summary>
        public int Type { get; set; }

        public string GetDescription()
        {
            var typeName = Type == 1 ? "上行" : "下行";
            return $"{IndexName}({IndexCode}) 于 {Date.ToString("yyyy-MM-dd")} 产生{typeName}拐点，目前处于{typeName}阶段";
        }
    }
}
