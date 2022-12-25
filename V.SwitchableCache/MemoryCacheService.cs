using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace V.SwitchableCache
{
    public class MemoryCacheService : ICacheService
    {
        private static MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        public void RemoveKey(string key) => _memoryCache.Remove(key);

        public string StringGet(string key) => _memoryCache.Get(key)?.ToString();

        public void StringSet(string key, string value, TimeSpan? expiry = null)
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
    }
}
