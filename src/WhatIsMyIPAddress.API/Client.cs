using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WhatIsMyIPAddress.API.Exceptions;
using WhatIsMyIPAddress.API.Models;

namespace WhatIsMyIPAddress.API
{
    public class Client
    {
        public IWebProxy Proxy { get; }

        private HttpClient httpClient;


        public Client([Optional]IWebProxy proxy)
        {
            Proxy = proxy;

            CreateHttpClient();
        }


        /// <summary>
        /// Provides your IP address.
        /// </summary>
        public IPAddress GetMyIPAddress(AddressFamily family = AddressFamily.InterNetwork)
        {
            var response = httpClient.GetString(Constants.BASE_URL);

            var ip = IPAddress.Parse(Regex.Matches(response, @"/ip/(?<ip>[a-z0-9.:]*?)""").Cast<Match>()
                .FirstOrDefault(m => !string.IsNullOrEmpty(m.Groups["ip"].Value))?.Groups["ip"].Value);

            if (ip.AddressFamily == family)
                return ip;
            else if (ip.AddressFamily == AddressFamily.InterNetworkV6 && family == AddressFamily.InterNetwork)
            {
                var ds3_url = Regex.Match(response, @"https://whatismyipaddress.com/ds3\?token=[a-z0-9]*?&v=4").Value;

                response = httpClient.GetString(ds3_url).Trim();

                if (response == "Not detected")
                    return IPAddress.Parse(httpClient.GetString(Constants.ICANHAZIP_URL).Trim());

                return IPAddress.Parse(response);
            }

            return ip;
        }

        /// <summary>
        /// Provides your IP address.
        /// </summary>
        public async Task<IPAddress> GetMyIPAddressAsync(AddressFamily family = AddressFamily.InterNetwork)
        {
            var response = await httpClient.GetStringAsync(Constants.BASE_URL);

            var ip = await Task.Run(() => IPAddress.Parse(Regex.Matches(response, @"/ip/(?<ip>[a-z0-9.:]*?)""").Cast<Match>()
                .FirstOrDefault(m => !string.IsNullOrEmpty(m.Groups["ip"].Value))?.Groups["ip"].Value));

            if (ip.AddressFamily == family)
                return ip;
            else if (ip.AddressFamily == AddressFamily.InterNetworkV6 && family == AddressFamily.InterNetwork)
            {
                var ds3_url = await Task.Run(() => Regex.Match(response, @"https://whatismyipaddress.com/ds3\?token=[a-z0-9]*?&v=4").Value);

                response = (await httpClient.GetStringAsync(ds3_url)).Trim();

                if (response == "Not detected")
                    return IPAddress.Parse((await httpClient.GetStringAsync(Constants.ICANHAZIP_URL)).Trim());

                return IPAddress.Parse(response);
            }

            return ip;
        }



        /// <summary>
        /// Provides details about an IP address.
        /// </summary>
        /// <param name="address">IP address</param>
        public IPDetails LookupIP(IPAddress address)
        {
            using (var client = GetHttpClient(allowAutoRedirect: true))
            {
                var document = client.GetHtmlDocument(string.Format(Constants.LOOKUP_IP_URL, address));

                return ExtractIPDetails(document);
            }
        }

        /// <summary>
        /// Provides details about an IP address.
        /// </summary>
        /// <param name="address">IP address</param>
        public async Task<IPDetails> LookupIPAsync(IPAddress address)
        {
            using (var client = GetHttpClient(allowAutoRedirect: true))
            {
                var document = await client.GetHtmlDocumentAsync(string.Format(Constants.LOOKUP_IP_URL, address));

                return await Task.Run(() => ExtractIPDetails(document));
            }
        }

        private static IPDetails ExtractIPDetails(HtmlDocument document)
        {
            var trElements = document.DocumentNode.Descendants("tr");

            var servicesElement = trElements.FirstOrDefault(tr => tr.FirstChild.InnerText == $"Services:")?.Descendants("td").First();
            var countryElement = trElements.FirstOrDefault(tr => tr.FirstChild.InnerText == $"Country:")?.Descendants("td").First();

            var ipDetails = new IPDetails
            {
                IP = ExtractAttributeValue(trElements, "IP"),
                Decimal = ExtractAttributeValue(trElements, "Decimal"),
                HostName = ExtractAttributeValue(trElements, "Hostname"),
                ASN = ExtractAttributeValue(trElements, "ASN"),
                ISP = ExtractAttributeValue(trElements, "ISP"),
                Organization = ExtractAttributeValue(trElements, "Organization"),

                Services = servicesElement.Descendants("a").Select(a => a.InnerText).ToList(),
                ServicesComment = servicesElement.LastChild.InnerText,

                Type = trElements.FirstOrDefault(tr => tr.FirstChild.InnerText == $"Type:")?
                    .Descendants("td").First().FirstChild.InnerText,

                Assignment = trElements.FirstOrDefault(tr => tr.FirstChild.InnerText == $"Assignment:")?
                    .Descendants("td").First().FirstChild.InnerText,

                Continent = ExtractAttributeValue(trElements, "Continent"),

                Country = countryElement.InnerText.Trim(),
                CountryFlagUrl = countryElement.Descendants("img").FirstOrDefault()?.GetAttributeValue("src", null).TrimStart('/'),

                Region = ExtractAttributeValue(trElements, "State/Region"),
                City = ExtractAttributeValue(trElements, "City"),
                Latitude = double.Parse(ExtractAttributeValue(trElements, "Latitude").Split('&')[0]),
                Longitude = double.Parse(ExtractAttributeValue(trElements, "Longitude").Split('&')[0]),
                PostalCode = ExtractAttributeValue(trElements, "Postal Code")
            };

            return ipDetails;
        }

        private static string ExtractAttributeValue(IEnumerable<HtmlNode> attributesNodes, string name)
        {
            return attributesNodes.FirstOrDefault(tr => tr.FirstChild.InnerText == $"{name}:")?
                .Descendants("td").First().InnerText.Trim();
        }



        /// <summary>
        /// Lists several of the test results that we perform to attempt to detect a proxy server.
        /// <para/>Some tests may result in a false positive for situations where there the IP being tested is a network sharing device.
        /// <para/>In some situations a proxy server is the normal circumstance (AOL users and users in some countries).
        /// </summary>
        /// <param name="proxy">Proxy to test</param>
        /// <param name="timeout">Proxy check request timeout</param>
        /// <param name="tries">Proxy check request max number of tries if request fails</param>
        public ProxyCheckResults ProxyCheck(IWebProxy proxy, [Optional]TimeSpan timeout, int tries = 1)
        {
            var document = Utilities.Utilities.TryMany(() => GetHttpClient(proxy, timeout).GetHtmlDocument(Constants.PROXY_CHECK_URL), tries);

            if (document == default(HtmlDocument))
                throw new NotWorkingProxyException(proxy);

            return ExtractProxyCheckResults(document);
        }

        /// <summary>
        /// Lists several of the test results that we perform to attempt to detect a proxy server.
        /// <para/>Some tests may result in a false positive for situations where there the IP being tested is a network sharing device.
        /// <para/>In some situations a proxy server is the normal circumstance (AOL users and users in some countries).
        /// </summary>
        /// <param name="proxy">Proxy to test</param>
        /// <param name="timeout">Proxy check request timeout</param>
        /// <param name="tries">Proxy check request max number of tries if request fails</param>
        public async Task<ProxyCheckResults> ProxyCheckAsync(IWebProxy proxy, [Optional]TimeSpan timeout, int tries = 1)
        {
            var document = await Utilities.Utilities.TryManyAsync(async () => await GetHttpClient(proxy, timeout).GetHtmlDocumentAsync(Constants.PROXY_CHECK_URL), tries);

            if (document == default(HtmlDocument))
                throw new NotWorkingProxyException(proxy);

            return await Task.Run(() => ExtractProxyCheckResults(document));
        }

        private static ProxyCheckResults ExtractProxyCheckResults(HtmlDocument document)
        {
            var trElements = document.DocumentNode.Descendants("tr");

            return new ProxyCheckResults
            {
                rDNS = bool.Parse(trElements.ElementAt(1).LastChild.InnerText.Trim()),
                WIMIA = bool.Parse(trElements.ElementAt(2).LastChild.InnerText.Trim()),
                Tor = bool.Parse(trElements.ElementAt(3).LastChild.InnerText.Trim()),
                Loc = bool.Parse(trElements.ElementAt(4).LastChild.InnerText.Trim()),
                Header = bool.Parse(trElements.ElementAt(5).LastChild.InnerText.Trim()),
                DNSBL = bool.Parse(trElements.ElementAt(6).LastChild.InnerText.Trim())
            };
        }



        /// <summary>
        /// Check to see if your IP address is listed with more than 100 DNSbl's as a machine that mail should not be accepted from.
        /// </summary>
        /// <param name="address">IP address</param>
        public BlacklistResult BlacklistCheck(IPAddress address)
        {
            httpClient.Get(Constants.BLACKLIST_CHECK_URL);

            HtmlDocument response;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, Constants.BLACKLIST_CHECK_URL))
            {
                requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "LOOKUPADDRESS", address.ToString() }, { "Lookup+Hostname", "Check+My+IP+Address" } });
                requestMessage.Headers.Referrer = new Uri(Constants.BLACKLIST_CHECK_URL);
                response = httpClient.Send(requestMessage).Content.ReadAsHtmlDocument();
            }

            var databases = ExtractDatabases(response);

            CheckDatabases(databases);

            return new BlacklistResult
            {
                Databases = databases
            };
        }

        /// <summary>
        /// Check to see if your IP address is listed with more than 100 DNSbl's as a machine that mail should not be accepted from.
        /// </summary>
        /// <param name="address">IP address</param>
        public async Task<BlacklistResult> BlacklistCheckAsync(IPAddress address)
        {
            await httpClient.GetAsync(Constants.BLACKLIST_CHECK_URL);

            HtmlDocument response;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, Constants.BLACKLIST_CHECK_URL))
            {
                requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "LOOKUPADDRESS", address.ToString() }, { "Lookup+Hostname", "Check+My+IP+Address" } });
                requestMessage.Headers.Referrer = new Uri(Constants.BLACKLIST_CHECK_URL);
                response = await (await httpClient.SendAsync(requestMessage)).Content.ReadAsHtmlDocumentAsync();
            }

            var databases = await Task.Run(() => ExtractDatabases(response));

            await CheckDatabasesAsync(databases);

            return new BlacklistResult
            {
                Databases = databases
            };
        }

        private static List<Database> ExtractDatabases(HtmlDocument document)
        {
            return document.DocumentNode.Descendants("table").First().Descendants("td").Where(td => td.HasChildNodes)
                .Select(td => new Database
                {
                    Name = td.LastChild.InnerText.Trim(),
                    Url = Constants.BASE_URL + WebUtility.HtmlDecode(td.Descendants("img").First().GetAttributeValue("src", null))
                }).ToList();
        }

        private void CheckDatabases(List<Database> databases)
        {
            List<Task> tasks = new List<Task>(); ;
            for (int i = 0; i < databases.Count; i++)
            {
                tasks.Add(CheckDatabaseAsync(databases[i]));
                Task.Delay(50).Wait();
            }

            try { Task.WhenAll(tasks).Wait(); }
            catch (Exception) { CheckDatabases(databases.Where(db => db.Value == DatabaseCheckResult.None).ToList()); }
        }

        private async Task CheckDatabasesAsync(List<Database> databases)
        {
            List<Task> tasks = new List<Task>(); ;
            for (int i = 0; i < databases.Count; i++)
            {
                tasks.Add(CheckDatabaseAsync(databases[i]));
                await Task.Delay(50);
            }

            try { await Task.WhenAll(tasks); }
            catch (Exception) { await CheckDatabasesAsync(databases.Where(db => db.Value == DatabaseCheckResult.None).ToList()); }
        }

        private void CheckDatabase(Database database)
        {
            using (var client = GetHttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Referer", Constants.BLACKLIST_CHECK_URL);
                client.DefaultRequestHeaders.Add("Host", "whatismyipaddress.com");
                //client.Headers.Add(HttpRequestHeader.Cookie, "fssts=false; fsbotchecked=true");

                HttpResponseMessage response;
                try
                {
                    response = Utilities.Utilities.TryMany(() => client.Get(new Uri(database.Url)), 2);
                }
                catch (Exception)
                {
                    database.Value = DatabaseCheckResult.None;
                    return;
                }
                
                var location = response.Headers.GetValues("Location").FirstOrDefault();

                if (location.Contains("green"))
                    database.Value = DatabaseCheckResult.Good;
                else if (location.Contains("red"))
                    database.Value = DatabaseCheckResult.Bad;
                else if (location.Contains("grey"))
                    database.Value = DatabaseCheckResult.Offline;
                else if (location.Contains("blue"))
                    database.Value = DatabaseCheckResult.Timeout;
            }
        }

        private async Task CheckDatabaseAsync(Database database)
        {
            using (var client = GetHttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Referer", Constants.BLACKLIST_CHECK_URL);
                client.DefaultRequestHeaders.Add("Host", "whatismyipaddress.com");
                //client.Headers.Add(HttpRequestHeader.Cookie, "fssts=false; fsbotchecked=true");

                HttpResponseMessage response;
                try
                {
                    response = await Utilities.Utilities.TryManyAsync(async () => await client.GetAsync(new Uri(database.Url)), 2);
                }
                catch (Exception)
                {
                    database.Value = DatabaseCheckResult.None;
                    return;
                }
                
                var location = response.Headers.GetValues("Location").FirstOrDefault();

                if (location.Contains("green"))
                    database.Value = DatabaseCheckResult.Good;
                else if (location.Contains("red"))
                    database.Value = DatabaseCheckResult.Bad;
                else if (location.Contains("grey"))
                    database.Value = DatabaseCheckResult.Offline;
                else if (location.Contains("blue"))
                    database.Value = DatabaseCheckResult.Timeout;
            }
        }



        /// <summary>
        /// Provides the hostname of an IP address. (ie 192.168.1.1)
        /// </summary>
        public string LookupHostname(IPAddress address)
        {
            httpClient.Get(Constants.IP_HOSTNAME_URL);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, Constants.IP_HOSTNAME_URL))
            {
                requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "LOOKUPADDRESS", address.ToString() }, { "Lookup+Hostname", "Lookup+Hostname" } });
                requestMessage.Headers.Referrer = new Uri(Constants.IP_HOSTNAME_URL);

                var response = httpClient.Send(requestMessage).Content.ReadAsString();

                return Regex.Match(response, @"Lookup Hostname: (?<hostname>.*?)<BR>").Groups["hostname"].Value;
            }
        }

        /// <summary>
        /// Provides the hostname of an IP address. (ie 192.168.1.1)
        /// </summary>
        public async Task<string> LookupHostnameAsync(IPAddress address)
        {
            await httpClient.GetAsync(Constants.IP_HOSTNAME_URL);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, Constants.IP_HOSTNAME_URL))
            {
                requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "LOOKUPADDRESS", address.ToString() }, { "Lookup+Hostname", "Lookup+Hostname" } });
                requestMessage.Headers.Referrer = new Uri(Constants.IP_HOSTNAME_URL);

                var response = await (await httpClient.SendAsync(requestMessage)).Content.ReadAsStringAsync();

                return Regex.Match(response, @"Lookup Hostname: (?<hostname>.*?)<BR>").Groups["hostname"].Value;
            }
        }



        /// <summary>
        /// Provides the IP address (or addresses, if applicable) of the hostname (ie www.yahoo.com)
        /// </summary>
        public List<IPAddress> LookupIPAddress(string hostname)
        {
            httpClient.Get(Constants.HOSTNAME_IP_URL);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, Constants.HOSTNAME_IP_URL))
            {
                requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "DOMAINNAME", hostname }, { "Lookup+IP+Address", "Lookup+IP+Address" } });
                requestMessage.Headers.Referrer = new Uri(Constants.HOSTNAME_IP_URL);

                var response = httpClient.Send(requestMessage).Content.ReadAsString();

                return Regex.Matches(response, @"Lookup IPv(4|6) Address: <a href='/ip/(?<ip>.*?)'>(?<ip>.*?)</a>")
                    .Cast<Match>().Select(m => IPAddress.Parse(m.Groups["ip"].Value)).ToList();
            }
        }

        /// <summary>
        /// Provides the IP address (or addresses, if applicable) of the hostname (ie www.yahoo.com).
        /// </summary>
        public async Task<List<IPAddress>> LookupIPAddressAsync(string hostname)
        {
            await httpClient.GetAsync(Constants.HOSTNAME_IP_URL);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, Constants.HOSTNAME_IP_URL))
            {
                requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "DOMAINNAME", hostname }, { "Lookup+IP+Address", "Lookup+IP+Address" } });
                requestMessage.Headers.Referrer = new Uri(Constants.HOSTNAME_IP_URL);

                var response = await (await httpClient.SendAsync(requestMessage)).Content.ReadAsStringAsync();

                return Regex.Matches(response, @"Lookup IPv(4|6) Address: <a href='/ip/(?<ip>.*?)'>(?<ip>.*?)</a>")
                    .Cast<Match>().Select(m => IPAddress.Parse(m.Groups["ip"].Value)).ToList();
            }
        }



        private HttpClient GetHttpClient([Optional]IWebProxy proxy, [Optional]TimeSpan timeout, [Optional]bool? allowAutoRedirect)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = allowAutoRedirect ?? false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                Proxy = proxy ?? Proxy ?? null
            };

            var httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.Add("User-Agent", Constants.USER_AGENT);

            if (timeout != default(TimeSpan))
                httpClient.Timeout = timeout;

            return httpClient;
        }

        private void CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                Proxy = Proxy ?? null
            };

            httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.Add("User-Agent", Constants.USER_AGENT);
        }

    }
}
