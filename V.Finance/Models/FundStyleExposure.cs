using System;
using System.Collections.Generic;
using System.Text;

namespace V.Finance.Models
{
    public class FundStyleExposure
    {
        public string FundCode { get; set; }

        public DateTime Date { get; set; }

        public double R1 { get; set; }

        public double R2 { get; set; }

        public double R3 { get; set; }

        public double R4 { get; set; }
    }
}
