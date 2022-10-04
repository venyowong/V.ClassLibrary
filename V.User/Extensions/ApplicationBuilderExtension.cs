using Dapper;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using V.User.OAuth;

namespace V.User.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseUserModule(this IApplicationBuilder app, bool useOAuth = false)
        {
            MakeDapperMapping("V.User.Models");

            app.UseMiddleware<UserMiddleware>();
            if (useOAuth)
            {
                app.UseOAuth();
            }
            return app;
        }

        /// <summary>
        /// 扫描命名空间下的 Dapper 映射
        /// <para>对于数据库字段与数据结构无法通过 Dapper 默认配置映射到一起的情况</para>
        /// <para>可在对应字段添加 ColumnAttribute 特性，参照 <see cref="Models.Feed"/></para>
        /// <para>并在程序启动后手动调用此接口，传入相应命名空间</para>
        /// </summary>
        /// <param name="namspace"></param>
        private static void MakeDapperMapping(string namspace)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(t => t.FullName.Contains(namspace)))
                {
                    var map = new CustomPropertyTypeMap(type, (t, columnName) => t.GetProperties().FirstOrDefault(
                        prop => GetDescriptionFromAttribute(prop) == columnName || prop.Name.ToLower().Equals(columnName.ToLower())));
                    SqlMapper.SetTypeMap(type, map);
                }
            }
        }

        private static string GetDescriptionFromAttribute(MemberInfo member)
        {
            if (member == null) return null;

            var attrib = (ColumnAttribute)Attribute.GetCustomAttribute(member, typeof(ColumnAttribute), false);
            return attrib == null ? null : attrib.Name;
        }
    }
}
