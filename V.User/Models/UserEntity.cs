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

        [Column("source_name")]
        public string SourceName { get; set; }

        [Column("platform_id")]
        public string PlatformId { get; set; }

        public string Mail { get; set; }

        public string Password { get; set; }

        public string Salt { get; set; }

        public string Location { get; set; }

        public string Company { get; set; }

        public string Bio { get; set; }

        [Column("mask_mobile")]
        public string MaskMobile { get; set; }

        [Column("md5_mobile")]
        public string Md5Mobile { get; set; }

        [Column("encrypted_mobile")]
        public string EncryptedMobile { get; set; }

        [Column("is_valid")]
        public bool IsValid { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        [Column("update_time")]
        public DateTime UpdateTime { get; set; }
    }
}
