using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace V.User.Models
{
    public class UserEntity
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("avatar")]
        public string Avatar { get; set; }

        [Column("gender")]
        public int Gender { get; set; }

        [Column("source")]
        public string Source { get; set; }

        [Column("source_name")]
        public string SourceName { get; set; }

        [Column("platform_id")]
        public string PlatformId { get; set; }

        [Column("mail")]
        public string Mail { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("salt")]
        public string Salt { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("company")]
        public string Company { get; set; }

        [Column("bio")]
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
