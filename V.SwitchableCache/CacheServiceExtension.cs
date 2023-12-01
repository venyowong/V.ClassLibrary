using System;
using System.Threading.Tasks;
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
            cacheService.StringSet(key, result.ToJson(), expiry);
            return result;
        }

        public static async Task<T> GetAsyncWithInit<T>(this ICacheService cacheService, string key, Func<Task<T>> init, TimeSpan? expiry = null)
        {
            var cache = cacheService.StringGet(key);
            if (!string.IsNullOrEmpty(cache))
            {
                return cache.ToObj<T>();
            }

            var result = await init();
            cacheService.StringSet(key, result.ToJson(), expiry);
            return result;
        }
    }
}