using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Zero.WhatIsMyIPAddress
{
    public class WhatIsMyIPAddress
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
        private const string LOOKUP_IP_URL = "http://whatismyipaddress.com/ip/{0}";
        private const string PROXY_CHECK_URL = "http://whatismyipaddress.com/proxy-check";

        /// <summary>
        /// Provides details about an IP address
        /// </summary>
        /// <param name="address">IP address</param>
        /// <returns>IP details</returns>
        public static IPDetails LookupIP(string address)
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", USER_AGENT);

            var url = string.Format(LOOKUP_IP_URL, address);
            var str = client.DownloadString(url);

            var document = new HtmlDocument();
            document.LoadHtml(str);
            
            var trElements = document.DocumentNode.Descendants("tr");

            var ipDetails = new IPDetails();

            ipDetails.IP = ExtractAttributeValue(trElements, "IP");
            ipDetails.Decimal = ExtractAttributeValue(trElements, "Decimal");
            ipDetails.HostName = ExtractAttributeValue(trElements, "Hostname");
            ipDetails.ASN = ExtractAttributeValue(trElements, "ASN");
            ipDetails.ISP = ExtractAttributeValue(trElements, "ISP");
            ipDetails.Organization = ExtractAttributeValue(trElements, "Organization");

            var servicesElement = trElements.FirstOrDefault(tr => tr.FirstChild.InnerText == $"Services:")?.Descendants("td").First();
            ipDetails.Services = servicesElement.Descendants("a").Select(a => a.InnerText).ToList();
            ipDetails.ServicesComment = servicesElement.LastChild.InnerText;

            ipDetails.Type = trElements.FirstOrDefault(tr => tr.FirstChild.InnerText == $"Type:")?
                .Descendants("td").First().FirstChild.InnerText;

            ipDetails.Assignment = trElements.FirstOrDefault(tr => tr.FirstChild.InnerText == $"Assignment:")?
                .Descendants("td").First().FirstChild.InnerText;

            ipDetails.Continent = ExtractAttributeValue(trElements, "Continent");

            var countryElement = trElements.FirstOrDefault(tr => tr.FirstChild.InnerText == $"Country:")?.Descendants("td").First();
            ipDetails.Country = countryElement.InnerText.Trim();
            ipDetails.CountryFlagUrl = countryElement.Descendants("img").FirstOrDefault()?.GetAttributeValue("src", null).TrimStart('/');

            ipDetails.Region = ExtractAttributeValue(trElements, "State/Region");
            ipDetails.City = ExtractAttributeValue(trElements, "City");
            ipDetails.Latitude = double.Parse(ExtractAttributeValue(trElements, "Latitude").Split('&')[0]);
            ipDetails.Longitude = double.Parse(ExtractAttributeValue(trElements, "Longitude").Split('&')[0]);
            ipDetails.PostalCode = ExtractAttributeValue(trElements, "Postal Code");

            return ipDetails;
        }

        private static string ExtractAttributeValue(IEnumerable<HtmlNode> attributesNodes, string name)
        {
            return attributesNodes.FirstOrDefault(tr => tr.FirstChild.InnerText == $"{name}:")?.Descendants("td").First().InnerText.Trim();
        }        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy">Proxy to test</param>
        /// <param name="timeout">Proxy check request timeout</param>
        /// <param name="tries">Proxy check request max number of tries if request fails</param>
        /// <returns>true if proxy detected</returns>
        public static ProxyCheckResults ProxyCheck(WebProxy proxy, int timeout = 20000, int tries = 1)
        {
            var response = Utilities.TryMany(() => Utilities.Get(PROXY_CHECK_URL, proxy, timeout), tries);

            if (response == default(string))
                throw new Exception("Proxy is not working or too slow.");

            var document = new HtmlDocument();
            document.LoadHtml(response);

            var trElements = document.DocumentNode.Descendants("tr").ToArray();

            return new ProxyCheckResults()
            {
                rDNS = bool.Parse(trElements[1].LastChild.InnerText.Trim()),
                WIMIA = bool.Parse(trElements[2].LastChild.InnerText.Trim()),
                Tor = bool.Parse(trElements[3].LastChild.InnerText.Trim()),
                Loc = bool.Parse(trElements[4].LastChild.InnerText.Trim()),
                Header = bool.Parse(trElements[5].LastChild.InnerText.Trim()),
                DNSBL = bool.Parse(trElements[6].LastChild.InnerText.Trim())
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy">Proxy to test</param>
        /// <param name="timeout">Proxy check request timeout</param>
        /// <param name="tries">Proxy check request max number of tries if request fails</param>
        /// <returns>true if proxy detected</returns>
        public static async Task<ProxyCheckResults> ProxyCheckAsync(WebProxy proxy, int timeout = 20000, int tries = 1)
        {
            var response = await Utilities.TryManyAsync(async () => await Utilities.GetAsync(PROXY_CHECK_URL, proxy, timeout), tries);
            
            if (response == default(string))
                throw new Exception("Proxy is not working or too slow.");

            var document = new HtmlDocument();
            document.LoadHtml(response);

            var trElements = document.DocumentNode.Descendants("tr").ToArray();

            return new ProxyCheckResults()
            {
                rDNS = bool.Parse(trElements[1].LastChild.InnerText.Trim()),
                WIMIA = bool.Parse(trElements[2].LastChild.InnerText.Trim()),
                Tor = bool.Parse(trElements[3].LastChild.InnerText.Trim()),
                Loc = bool.Parse(trElements[4].LastChild.InnerText.Trim()),
                Header = bool.Parse(trElements[5].LastChild.InnerText.Trim()),
                DNSBL = bool.Parse(trElements[6].LastChild.InnerText.Trim())
            };
        }


    }
}