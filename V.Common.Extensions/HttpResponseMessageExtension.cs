using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace V.Common.Extensions
{
    public static class HttpResponseMessageExtension
    {
        public static async Task<T> ReadAsObj<T>(this HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return default;
            }
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var json = await response.Content.ReadAsStringAsync();
            return json.ToObj<T>();
        }

        public static async Task<string> ReadAsString(this HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return default;
            }
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> ReadAsDecompressedString(this HttpResponseMessage response)
        {
            if (!response?.IsSuccessStatusCode ?? false)
            {
                return string.Empty;
            }
            if (response?.Content == null)
            {
                return string.Empty;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return Encoding.UTF8.GetString(GZipDecompress(bytes));
        }

        private static byte[] GZipDecompress(byte[] zippedData)
        {
            try
            {
                using (var stream = new GZipStream(new MemoryStream(zippedData), CompressionMode.Decompress))
                {
                    const int size = 4096;
                    var buffer = new byte[size];
                    using (var memory = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0)
                            {
                                memory.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);
                        return memory.ToArray();
                    }
                }
            }
            catch
            {
                return new byte[0];
            }
        }
    }
}
