using System.Net;
using System.Threading.Tasks;
using WhatIsMyIPAddress.API;

namespace WhatIsMyIPAddress.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            //Synchronous Test
            Test();

            //Asynchronous Test
            //TestAsync().Wait();
        }


        private static void Test()
        {
            var client = new Client(/*new WebProxy("169.57.1.84:8080")*/);

            var ip = client.GetMyIPAddress();

            var details = client.LookupIP(ip);

            var proxy = client.ProxyCheck(new WebProxy("138.68.240.218:8080")).IsProxyServer;

            var blacklist = client.BlacklistCheck(ip);

            var hostname = client.LookupHostname(ip);

            var resultIPs = client.LookupIPAddress("www.yahoo.com");
        }

        private async static Task TestAsync()
        {
            var client = new Client(/*new WebProxy("169.57.1.84:8080")*/);

            var ip = await client.GetMyIPAddressAsync();

            var details = await client.LookupIPAsync(ip);

            var proxy = (await client.ProxyCheckAsync(new WebProxy("138.68.240.218:8080"))).IsProxyServer;

            var blacklist = await client.BlacklistCheckAsync(ip);

            var hostname = await client.LookupHostnameAsync(ip);

            var resultIPs = await client.LookupIPAddressAsync("www.yahoo.com");
        }
    }
}
