using Dapper;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace V.Dapper.Extension
{
    public static class DapperHelper
    {
        /// <summary>
        /// 扫描命名空间下的 Dapper 映射
        /// <para>对于数据库字段与数据结构无法通过 Dapper 默认配置映射到一起的情况</para>
        /// <para>可在对应字段添加 ColumnAttribute 特性
        /// <para>并在程序启动后手动调用此接口，传入相应命名空间</para>
        /// </summary>
        /// <param name="namspace"></param>
        public static void MakeDapperMapping(string namspace)
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
