using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using V.User.Models;

namespace V.User.Services
{
    public class UserDao
    {
        private QueryFactory db;

        public UserDao(QueryFactory db)
        {
            this.db = db;
        }

        public Task<UserEntity> GetUserByMobile(string md5Mobile) => this.db.Query("public.user")
            .Where("md5_mobile", md5Mobile)
            .WhereTrue("is_valid")
            .FirstOrDefaultAsync<UserEntity>();

        public Task<UserEntity> GetUserByMail(string mail) => this.db.Query("public.user")
            .Where("mail", mail)
            .WhereTrue("is_valid")
            .FirstOrDefaultAsync<UserEntity>();

        public Task<UserEntity> GetUserByPlatform(string source, string platformId) => this.db.Query("public.user")
            .Where("source", source)
            .Where("platform_id", platformId)
            .WhereTrue("is_valid")
            .FirstOrDefaultAsync<UserEntity>();

        public Task<UserEntity> GetUser(long id) => this.db.Query("public.user")
            .Where("id", id)
            .WhereTrue("is_valid")
            .FirstOrDefaultAsync<UserEntity>();

        public Task<long> InsertUser(UserEntity user) => this.db.Query("public.user")
            .InsertGetIdAsync<long>(new
            {
                name = user.Name,
                avatar = user.Avatar,
                source = user.Source,
                source_name = user.SourceName,
                platform_id = user.PlatformId,
                mail = user.Mail,
                location = user.Location,
                company = user.Company,
                bio = user.Bio,
                gender = user.Gender,
                password = user.Password,
                salt = user.Salt,
                mask_mobile = user.MaskMobile,
                md5_mobile = user.Md5Mobile,
                encrypted_mobile = user.EncryptedMobile
            });

        public async Task<bool> UpdateUser(UserEntity user)
        {
            var result = await this.db.Query("public.user")
                .Where("id", user.Id)
                .UpdateAsync(new
                {
                    name = user.Name,
                    avatar = user.Avatar,
                    source = user.Source,
                    source_name = user.SourceName,
                    platform_id = user.PlatformId,
                    mail = user.Mail,
                    location = user.Location,
                    company = user.Company,
                    bio = user.Bio,
                    gender = user.Gender,
                    password = user.Password,
                    salt = user.Salt,
                    mask_mobile = user.MaskMobile,
                    md5_mobile = user.Md5Mobile,
                    encrypted_mobile = user.EncryptedMobile,
                    update_time = DateTime.Now
                });
            return result > 0;
        }
    }
}
