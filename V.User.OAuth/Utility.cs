using System;
using System.Collections.Generic;
using System.Text;

namespace V.User.OAuth
{
    public static class Utility
    {
        /// <summary>
        /// 获取服务所对应的产品名称
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static string GetServiceProductName(string serviceName)
        {
            switch (serviceName)
            {
                case "gitee":
                    return "Gitee";
                case "github":
                    return "Github";
                case "stackexchange":
                    return "StackExchange";
                default:
                    return string.Empty;
            }
        }
    }
}
