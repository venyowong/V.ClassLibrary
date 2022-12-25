using System;

namespace V.SwitchableCache
{
    public interface ICacheService
    {
        void StringSet(string key, string value, TimeSpan? expiry = null);

        string StringGet(string key);

        void RemoveKey(string key);
    }
}
