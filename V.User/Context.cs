using System;
using System.Collections.Generic;
using System.Text;

namespace V.User
{
    internal class Context
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Step { get; set; }

        public string VerificationCode { get; set; }

        public string Mail { get; set; }

        public string Mobile { get; set; }

        public long UserId { get; set; }

        public DateTime Expiration { get; set; }
    }
}
