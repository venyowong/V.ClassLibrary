using System;
using System.Collections.Generic;
using System.Text;

namespace V.Common.Extensions
{
    public static class Converter
    {
        /// <summary>
        /// 将字符串转换为基础类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object FromString(string value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return Convert.ToBoolean(value);
                case TypeCode.Byte:
                    return Convert.ToByte(value);
                case TypeCode.Char:
                    return Convert.ToChar(value);
                case TypeCode.DateTime:
                    return Convert.ToDateTime(value);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(value);
                case TypeCode.Double:
                    return Convert.ToDouble(value);
                case TypeCode.Int16:
                    return Convert.ToInt16(value);
                case TypeCode.Int32:
                    return Convert.ToInt32(value);
                case TypeCode.Int64:
                    return Convert.ToInt64(value);
                case TypeCode.SByte:
                    return Convert.ToSByte(value);
                case TypeCode.Single:
                    return Convert.ToSingle(value);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(value);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(value);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value);
                default:
                    return value;
            }
        }

        /// <summary>
        /// 将毫米级时间戳转化为 datetime
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DateTime ToDateTimeFromMilliseconds(double milliseconds)
        {
            var time = new DateTime(1970, 1, 1).ToLocalTime();
            return time.AddMilliseconds(milliseconds);
        }
    }
}
