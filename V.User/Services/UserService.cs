using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.SwitchableCache;
using V.User.Models;

namespace V.User.Services
{
    public class UserService
    {
        private ICacheService cacheService;
        private UserDao dao;

        public UserService(ICacheService cacheService, UserDao dao)
        {
            this.cacheService = cacheService;
            this.dao = dao;
        }

        public async Task<UserEntity> GetUserByMobile(string md5Mobile)
        {
            var key = "V:User:Services:UserService:GetUserByMobile:" + md5Mobile;
            var json = this.cacheService.StringGet(key);
            if (!string.IsNullOrWhiteSpace(json))
            {
                return json.ToObj<UserEntity>();
            }

            var user = await this.dao.GetUserByMobile(md5Mobile);
            if (user == null)
            {
                return null;
            }

            this.cacheService.StringSet(key, user.ToJson(), new TimeSpan(0, 10, 0));
            return user;
        }

        public async Task<UserEntity> GetUserByMail(string mail)
        {
            var key = "V:User:Services:UserService:GetUserByMail:" + mail;
            var json = this.cacheService.StringGet(key);
            if (!string.IsNullOrWhiteSpace(json))
            {
                return json.ToObj<UserEntity>();
            }

            var user = await this.dao.GetUserByMail(mail);
            if (user == null)
            {
                return null;
            }

            this.cacheService.StringSet(key, user.ToJson(), new TimeSpan(0, 10, 0));
            return user;
        }

        public async Task<UserEntity> GetUserByPlatform(string source, string platformId)
        {
            var key = $"V:User:Services:UserService:GetUserByPlatform:{source}:{platformId}";
            var json = this.cacheService.StringGet(key);
            if (!string.IsNullOrWhiteSpace(json))
            {
                return json.ToObj<UserEntity>();
            }

            var user = await this.dao.GetUserByPlatform(source, platformId);
            if (user == null)
            {
                return null;
            }

            this.cacheService.StringSet(key, user.ToJson(), new TimeSpan(0, 10, 0));
            return user;
        }

        public async Task<UserEntity> GetUser(long id)
        {
            var key = "V:User:Services:UserService:GetUser:" + id;
            var json = this.cacheService.StringGet(key);
            if (!string.IsNullOrWhiteSpace(json))
            {
                return json.ToObj<UserEntity>();
            }

            var user = await this.dao.GetUser(id);
            if (user == null)
            {
                return null;
            }

            this.cacheService.StringSet(key, user.ToJson(), new TimeSpan(0, 10, 0));
            return user;
        }

        public async Task<long> CreateUser(UserEntity user)
        {
            var id = await this.dao.InsertUser(user);
            if (id <= 0)
            {
                return id;
            }

            user.Id = id;
            this.RemoveCache(user);
            return id;
        }

        public async Task<bool> UpdateUser(UserEntity user)
        {
            var result = await this.dao.UpdateUser(user);
            if (!result)
            {
                return result;
            }

            this.RemoveCache(user);
            return result;
        }

        private void RemoveCache(UserEntity user)
        {
            if (!string.IsNullOrWhiteSpace(user.Mail))
            {
                this.cacheService.RemoveKey("V:User:Services:UserService:GetUserByMail:" + user.Mail);
            }
            if (!string.IsNullOrWhiteSpace(user.Md5Mobile))
            {
                this.cacheService.RemoveKey("V:User:Services:UserService:GetUserByMobile:" + user.Md5Mobile);
            }
            if (!string.IsNullOrWhiteSpace(user.PlatformId))
            {
                this.cacheService.RemoveKey($"V:User:Services:UserService:GetUserByPlatform:{user.Source}:{user.PlatformId}");
            }
            this.cacheService.RemoveKey("V:User:Services:UserService:GetUser:" + user.Id);
        }
    }
}
