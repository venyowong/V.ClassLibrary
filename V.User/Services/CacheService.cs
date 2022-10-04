using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace V.User.Services
{
    public class CacheService
    {
        private Configuration config;
        private IDatabase redis;

        private static MemoryCache _memoryCache = null;
        private static ConnectionMultiplexer _connection = null;

        public CacheService(Configuration config)
        {
            this.config = config;
            if (config.CacheMode == 1)
            {
                if (_connection == null)
                {
                    _connection = ConnectionMultiplexer.Connect(config.RedisConnectionString);
                }
                this.redis = _connection.GetDatabase(this.config.RedisDb);
            }
            else
            {
                if (_memoryCache == null)
                {
                    _memoryCache = new MemoryCache(new MemoryCacheOptions());
                }
            }
        }

        public void StringSet(string key, string value, TimeSpan? expiry = null)
        {
            if (config.CacheMode == 0)
            {
                if (expiry == null || expiry.Value.TotalSeconds <= 0)
                {
                    _memoryCache.Set(key, value);
                }
                else
                {
                    _memoryCache.Set(key, value, expiry.Value);
                }
            }
            else
            {
                this.redis.StringSet(key, value, expiry);
            }
        }

        public string StringGet(string key)
        {
            if (config.CacheMode == 0)
            {
                return _memoryCache.Get(key)?.ToString();
            }
            else
            {
                return this.redis.StringGet(key);
            }
        }

        public void RemoveKey(string key)
        {
            if (config.CacheMode == 0)
            {
                _memoryCache.Remove(key);
            }
            else
            {
                this.redis.KeyDelete(key);
            }
        }
    }
}
