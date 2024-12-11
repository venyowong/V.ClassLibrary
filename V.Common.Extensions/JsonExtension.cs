using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace V.Common.Extensions
{
    public static class JsonExtension
    {
        public static T Get<T>(this JToken jToken, string path)
        {
            if (jToken == null)
            {
                return default;
            }

            var paths = path.Split(':');
            JToken token = null;
            foreach (var p in paths)
            {
                token = token == null ? jToken[p] : token[p];
            }
            if (token == null)
            {
                return default;
            }

            return token.ToObject<T>();
        }
    }
}
