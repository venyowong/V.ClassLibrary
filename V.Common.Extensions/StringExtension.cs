using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace V.Common.Extensions
{
    public static class StringExtension
    {
        private static JsonSerializerSettings _setting = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private static Regex _mobileRegex = new Regex(@"^1(3\d|4[5-9]|5[0-35-9]|6[567]|7[0-8]|8\d|9[0-35-9])\d{8}$");

        public static string ToJson(this object obj, bool useCamel = true, Formatting formatting = Formatting.None)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            if (useCamel)
            {
                return JsonConvert.SerializeObject(obj, formatting, _setting);
            }
            else
            {
                return JsonConvert.SerializeObject(obj, formatting);
            }
        }

        public static T ToObj<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Md5(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            using (var md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        public static string MaskMobile(this string mobile) => $"{mobile.Substring(0, 3)}****{mobile.Substring(7)}";

        public static bool IsValidMobile(this string mobile) =>  _mobileRegex.IsMatch(mobile);

        public static string Sha1(this string content)
        {
            using (var sha1 = SHA1.Create())
            {
                return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(content))).Replace("-", "");
            }
        }

        #region DES
        private static byte[] _rgbKey = Encoding.ASCII.GetBytes("670851ad");
        private static byte[] _rgbIV = Encoding.ASCII.GetBytes("89532a19");

        /// <summary>
        /// DES 加密
        /// </summary>
        /// <param name="text">需要加密的值</param>
        /// <returns>加密后的结果</returns>
        public static string DESEncrypt(this string text, byte[] key = null, byte[] iv = null)
        {
            if (key == null)
            {
                key = _rgbKey;
            }
            if (iv == null)
            {
                iv = _rgbIV;
            }

            using (var dsp = new DESCryptoServiceProvider())
            using (var memStream = new MemoryStream())
            {
                var crypStream = new CryptoStream(memStream, dsp.CreateEncryptor(key, iv), CryptoStreamMode.Write);
                var sWriter = new StreamWriter(crypStream);
                sWriter.Write(text);
                sWriter.Flush();
                crypStream.FlushFinalBlock();
                memStream.Flush();
                return Convert.ToBase64String(memStream.GetBuffer(), 0, (int)memStream.Length);
            }
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="encryptText"></param>
        /// <returns>解密后的结果</returns>
        public static string DESDecrypt(this string encryptText, byte[] key = null, byte[] iv = null)
        {
            if (key == null)
            {
                key = _rgbKey;
            }
            if (iv == null)
            {
                iv = _rgbIV;
            }
            var buffer = Convert.FromBase64String(encryptText);

            using (var dsp = new DESCryptoServiceProvider())
            using (var memStream = new MemoryStream())
            {
                var crypStream = new CryptoStream(memStream, dsp.CreateDecryptor(key, iv), CryptoStreamMode.Write);
                crypStream.Write(buffer, 0, buffer.Length);
                crypStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(memStream.ToArray());
            }
        }
        #endregion

        /// <summary>
        /// 解析日期字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="format"></param>
        /// <returns>解析失败则返回 default(DateTime)</returns>
        public static DateTime TryParseDateTime(this string str, string format = null)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                DateTime.TryParse(str, out DateTime result);
                return result;
            }
            else
            {
                DateTime.TryParseExact(str, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
                return result;
            }
        }
    }
}
