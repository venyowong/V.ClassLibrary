using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace V.SwitchableCache
{
    public class RedisCacheService : ICacheService
    {
        private IDatabase redis;

        public RedisCacheService(ConnectionMultiplexer connection, int db)
        {
            this.redis = connection.GetDatabase(db);
        }

        public void RemoveKey(string key) => this.redis.KeyDelete(key);

        public string StringGet(string key) => this.redis.StringGet(key);

        public void StringSet(string key, string value, TimeSpan? expiry = null) => this.redis.StringSet(key, value, expiry);
    }
}
