using System.Net;
using V.Common.Extensions;
using WebDav;

namespace V.WebDav
{
    public class WebDavService : IDisposable
    {
        private WebDavClient client;

        public WebDavService(string userName, string password, string baseAddress) 
        { 
            this.client = new WebDavClient(new HttpClient(new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                Credentials = new NetworkCredential(userName, password)
            }, true)
            { BaseAddress = new Uri(baseAddress) });
        }

        public async Task<string> Get(string path)
        {
            using var response = await this.client.GetProcessedFile(path);
            using var reader = new StreamReader(response.Stream);
            return reader.ReadToEnd();
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public async Task<T> Get<T>(string path)
        {
            var result = await this.Get(path);
            return result.ToObj<T>();
        }
    }
}
