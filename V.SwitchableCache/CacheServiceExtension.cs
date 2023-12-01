using System;
using V.Common.Extensions;

namespace V.SwitchableCache
{
    public static class CacheServiceExtension
    {
        public static T GetWithInit<T>(this ICacheService cacheService, string key, Func<T> init, TimeSpan? expiry = null)
        {
            var cache = cacheService.StringGet(key);
            if (!string.IsNullOrEmpty(cache))
            {
                return cache.ToObj<T>();
            }

            var result = init();
            cacheService.StringSet(key, result.ToString(), expiry);
            return result;
        }
    }
}