using System;
using System.Net;

#if NETSTANDARD1_3
using System.Net.Proxy;
#endif

namespace WhatIsMyIPAddress.API.Exceptions
{
    public class NotWorkingProxyException : Exception
    {

        public readonly WebProxy Proxy;

        public NotWorkingProxyException()
        {

        }

        public NotWorkingProxyException(IWebProxy proxy)
            : base($"Proxy: {((WebProxy)proxy).Address}, is not working or too slow.")
        {
            Proxy = (WebProxy)proxy;
        }
    }
}
