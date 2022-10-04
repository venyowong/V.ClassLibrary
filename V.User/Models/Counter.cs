using System;
using System.Collections.Generic;
using System.Text;

namespace V.User.Models
{
    public class Counter
    {
        public int Count { get; set; }

        public DateTime Expiration { get; set; }

        public Counter(DateTime? expiration = null)
        {
            if (expiration == null)
            {
                this.Expiration = DateTime.Now.Date.AddDays(1);
            }
            else
            {
                this.Expiration = expiration.Value;
            }
        }

        public void Inc()
        {
            var now = DateTime.Now;
            if (now >= this.Expiration)
            {
                this.Count = 0;
                this.Expiration = now.Date.AddDays(1);
            }
            this.Count++;
        }

        public bool Over(int limit)
        {
            if (DateTime.Now < this.Expiration && this.Count >= limit)
            {
                return true;
            }

            return false;
        }
    }
}
