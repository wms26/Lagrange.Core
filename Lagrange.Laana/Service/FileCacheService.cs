using Laana;
using Lagrange.Laana.Common;

namespace Lagrange.Laana.Service
{
    public interface IFileCacheService
    {
        Task<string> PrepareCacheFromUrl(
            string url, Dictionary<string, string> headers, PrepareCacheByUrlPing.Types.Method method);
        
        Task<string> PrepareCacheFromBytes(byte[] data);
        
        Task<byte[]> ResolveIncomingLaanaFile(LaanaFile laanaFile);
        
        void DestroyCache(string cacheId);
        
        string ResolveCachePath(string cacheId);
    }
    
    public sealed class FileCacheService(string cachePath) : IFileCacheService
    {
        public async Task<string> PrepareCacheFromUrl(
            string url, Dictionary<string, string> headers, PrepareCacheByUrlPing.Types.Method method)
        {
            string cacheId = $"from-url-{url.CalculateMd5()}";
            if (File.Exists(Path.Combine(cachePath, cacheId)))
            {
                return Path.Combine(cachePath, cacheId);
            }

            byte[] content = await DownloadFile(url, headers, method);
            await File.WriteAllBytesAsync(Path.Combine(cachePath, cacheId), content);

            return cacheId;
        }

        public async Task<string> PrepareCacheFromBytes(byte[] data)
        {
            string cacheId = $"from-data-{data.CalculateMd5()}";
            if (File.Exists(Path.Combine(cachePath, cacheId)))
            {
                return cacheId;
            }

            await File.WriteAllBytesAsync(Path.Combine(cachePath, cacheId), data);
            return cacheId;
        }

        public async Task<byte[]> ResolveIncomingLaanaFile(LaanaFile laanaFile)
        {
            return laanaFile.UriCase switch
            {
                LaanaFile.UriOneofCase.Url => await DownloadFile(
                    laanaFile.Url, [], PrepareCacheByUrlPing.Types.Method.Get),
                LaanaFile.UriOneofCase.Raw => laanaFile.Raw.ToByteArray(),
                LaanaFile.UriOneofCase.CacheId => await File.ReadAllBytesAsync(ResolveCachePath(laanaFile.CacheId)),
                _ => throw new Exception("Unsupported URI type.")
            };
        }

        public void DestroyCache(string cacheId)
        {
            if (File.Exists(Path.Combine(cachePath, cacheId)))
            {
                File.Delete(Path.Combine(cachePath, cacheId));
            }
        }

        public string ResolveCachePath(string cacheId)
        {
            return Path.Combine(cachePath, cacheId);
        }

        private static async Task<byte[]> DownloadFile(string url, Dictionary<string, string> headers,
            PrepareCacheByUrlPing.Types.Method method)
        {
            using var client = new HttpClient();
            foreach (var (key, value) in headers)
            {
                client.DefaultRequestHeaders.Add(key, value);
            }

            var response = method switch
            {
                PrepareCacheByUrlPing.Types.Method.Get => await client.GetAsync(url),
                PrepareCacheByUrlPing.Types.Method.Post => await client.PostAsync(url, new StringContent("")),
                PrepareCacheByUrlPing.Types.Method.Put => await client.PutAsync(url, new StringContent("")),
                PrepareCacheByUrlPing.Types.Method.Delete => await client.DeleteAsync(url),
                _ => throw new Exception("Unsupported method.")
            };
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to download {url}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}