using Dapper;
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
        private IDbConnection conn;

        public UserDao(IDbConnection connection)
        {
            this.conn = connection;
        }

        public Task<UserEntity> GetUserByMobile(string md5Mobile) => this.conn.QueryFirstOrDefaultAsync<UserEntity>("SELECT * FROM public.user WHERE md5_mobile=@md5Mobile AND is_valid=true", new { md5Mobile });

        public Task<UserEntity> GetUserByMail(string mail) => this.conn.QueryFirstOrDefaultAsync<UserEntity>("SELECT * FROM public.user WHERE mail=@mail AND is_valid=true", new { mail });

        public Task<UserEntity> GetUserByPlatform(string source, string platformId) =>
            this.conn.QueryFirstOrDefaultAsync<UserEntity>("SELECT * FROM public.user WHERE source=@source AND platform_id=@platformId AND is_valid=true", new { source, platformId });

        public Task<UserEntity> GetUser(long id) => this.conn.QueryFirstOrDefaultAsync<UserEntity>($"SELECT * FROM public.user WHERE id={id} AND is_valid=true");

        public Task<long> InsertUser(UserEntity user) => this.conn.ExecuteScalarAsync<long>(
            @"INSERT INTO public.user(name, avatar, source, source_name, platform_id, mail, location, company, bio, gender, password, salt, mask_mobile, md5_mobile, encrypted_mobile) 
            VALUES(@Name, @Avatar, @Source, @SourceName, @PlatformId, @Mail, @Location, @Company, @Bio, @Gender, @Password, @Salt, @MaskMobile, @Md5Mobile, @EncryptedMobile) 
            RETURNING id", user);

        public async Task<bool> UpdateUser(UserEntity user)
        {
            var result = await this.conn.ExecuteAsync(@"UPDATE public.user   SET name=@Name, avatar=@Avatar, source=@Source, source_name=@SourceName,
                platform_id=@PlatformId, mail=@Mail, location=@Location, company=@Company, bio=@Bio, gender=@Gender, password=@Password, 
                salt=@Salt, update_time=NOW(), mask_mobile=@MaskMobile, md5_mobile=@Md5Mobile, encrypted_mobile=@EncryptedMobile WHERE id=@Id", user);
            return result > 0;
        }
    }
}
