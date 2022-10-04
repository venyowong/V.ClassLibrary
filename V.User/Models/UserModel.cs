using System;
using System.Collections.Generic;
using System.Text;

namespace V.User.Models
{
    public class UserModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Avatar { get; set; }

        public int Gender { get; set; }

        public string Source { get; set; }

        public string SourceName { get; set; }

        public string PlatformId { get; set; }

        public string Mail { get; set; }

        public string Location { get; set; }

        public string Company { get; set; }

        public string Bio { get; set; }

        public string MaskMobile { get; set; }

        public string Md5Mobile { get; set; }

        public string EncryptedMobile { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string Token { get; set; }

        /// <summary>
        /// 是否可以设置密码
        /// </summary>
        public bool CanSetPwd { get; set; }

        public UserModel() { }

        public UserModel(UserEntity user)
        {
            this.Id = user.Id;
            this.Avatar = user.Avatar;
            this.Bio = user.Bio;
            this.Company = user.Company;
            this.CreateTime = user.CreateTime;
            this.EncryptedMobile = user.EncryptedMobile;
            this.Gender = user.Gender;
            this.Location = user.Location;
            this.Mail = user.Mail;
            this.MaskMobile = user.MaskMobile;
            this.Md5Mobile = user.Md5Mobile;
            this.Name = user.Name;
            this.PlatformId = user.PlatformId;
            this.SourceName = user.SourceName;
            this.Source = user.Source;
            this.UpdateTime = user.UpdateTime;
            this.CanSetPwd = string.IsNullOrWhiteSpace(user.Password) ? true : false;
        }
    }
}
