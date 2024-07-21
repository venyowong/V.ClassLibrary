using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class FundNav
    {
        public string FundCode { get; set; }

        public decimal UnitNav { get; set; }

        public decimal AccUnitNav { get; set; }

        public DateTime Date { get; set; }
    }
}
