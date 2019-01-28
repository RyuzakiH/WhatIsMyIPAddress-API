using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Zero.WhatIsMyIPAddress
{
    public class Utilities
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";


        public static string Get(string url, WebProxy proxy = null, int timeout = 20000)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.UserAgent = USER_AGENT;
            request.Timeout = timeout;
            request.Proxy = proxy;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        public static async Task<string> GetAsync(string url, WebProxy proxy = null, int timeout = 20000)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.UserAgent = USER_AGENT;
            request.Timeout = timeout;
            request.Proxy = proxy;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
                return await reader.ReadToEndAsync();
        }


        public static T TryMany<T>(Func<T> func, int tries)
        {
            T result = default(T);

            for (int i = 0; i < tries; i++)
            {
                try { result = func(); break; }
                catch (Exception) { continue; }
            }

            return result;
        }

        public async static Task<T> TryManyAsync<T>(Func<Task<T>> func, int tries)
        {
            T result = default(T);

            for (int i = 0; i < tries; i++)
            {
                try { result = await func(); break; }
                catch (Exception) { continue; }
            }

            return result;
        }

    }
}