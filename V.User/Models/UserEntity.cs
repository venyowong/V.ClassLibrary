using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace V.User.Models
{
    public class UserEntity
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Avatar { get; set; }

        public int Gender { get; set; }

        public string Source { get; set; }

        public string SourceName { get; set; }

        public string PlatformId { get; set; }

        public string Mail { get; set; }

        public string Password { get; set; }

        public string Salt { get; set; }

        public string Location { get; set; }

        public string Company { get; set; }

        public string Bio { get; set; }

        public string MaskMobile { get; set; }

        public string Md5Mobile { get; set; }

        public string EncryptedMobile { get; set; }

        public bool IsValid { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
