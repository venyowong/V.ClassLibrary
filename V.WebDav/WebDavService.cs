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

        public async Task<T> Get<T>(string path)
        {
            var result = await this.Get(path);
            return result.ToObj<T>();
        }

        public async Task<List<NoteItem>> GetNotes(string path)
        {
            var note = await this.Get(path);
            if (string.IsNullOrEmpty(note))
            {
                return null;
            }

            try
            {
                var list = note.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                var result = new List<NoteItem>();
                var parent = new List<NoteItem>();
                foreach (var item in list)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }

                    var text = item.TrimStart();
                    var layers = (item.Length - text.Length) / 2;
                    if (text.StartsWith('-'))
                    {
                        var noteItem = new NoteItem
                        {
                            Title = text[1..].Trim(),
                            Children = []
                        };
                        if (layers > parent.Count - 1)
                        {
                            if (layers == 0)
                            {
                                result.Add(noteItem);
                                parent.Add(noteItem);
                            }
                            else
                            {
                                parent[layers - 1].Children.Add(noteItem);
                                parent.Add(noteItem);
                            }
                        }
                        else if (layers == parent.Count - 1)
                        {
                            if (parent.Count == 1)
                            {
                                result.Add(noteItem);
                                parent[0] = noteItem;
                            }
                            else
                            {
                                parent[layers - 1].Children.Add(noteItem);
                                parent[layers] = noteItem;
                            }
                        }
                        else
                        {
                            if (layers == 0)
                            {
                                result.Add(noteItem);
                                parent[0] = noteItem;
                            }
                            else
                            {
                                parent[layers - 1].Children.Add(noteItem);
                                parent[layers] = noteItem;
                            }

                            for (int i = parent.Count - 1; i > layers; i--)
                            {
                                parent.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        layers--;
                        if (layers < 0 || layers >= parent.Count)
                        {
                            continue;
                        }

                        var p = parent[layers];
                        if (string.IsNullOrEmpty(p.Description))
                        {
                            p.Description = text;
                        }
                        else
                        {
                            p.Description = $"{p.Description}\n{text}";
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception($"{path} 格式不规范", e);
            }
        }

        public void Dispose()
        {
            this.client.Dispose();
        }
    }
}
