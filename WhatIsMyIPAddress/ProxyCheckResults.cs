namespace Zero.WhatIsMyIPAddress
{
    public class ProxyCheckResults
    {
        public bool rDNS { get; set; }
        public bool WIMIA { get; set; }
        public bool Tor { get; set; }
        public bool Loc { get; set; }
        public bool Header { get; set; }
        public bool DNSBL { get; set; }

        /// <summary>
        /// Gets Whether a proxy server is detected or not
        /// </summary>
        public bool IsProxyServer
        {
            get
            {
                return rDNS || WIMIA || Tor || Loc || Header || DNSBL;
            }
        }

        public ProxyCheckResults()
        {
            rDNS = WIMIA = Tor = Loc = Header = DNSBL = false;
        }

    }
}