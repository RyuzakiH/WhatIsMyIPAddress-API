WhatIsMyIPAddress-API
[![AppVeyor](https://img.shields.io/appveyor/ci/RyuzakiH/WhatIsMyIPAddress-API/master.svg?maxAge=60)](https://ci.appveyor.com/project/RyuzakiH/WhatIsMyIPAddress-API)
[![NuGet](https://img.shields.io/nuget/v/WhatIsMyIPAddress.API.svg?maxAge=60)](https://www.nuget.org/packages/WhatIsMyIPAddress.API)
===============

Unofficial API for [WhatIsMyIPAddress](https://whatismyipaddress.com) in .NET Standard

**NuGet**: https://www.nuget.org/packages/WhatIsMyIPAddress.API

# Usage

This API provides synchronous and asynchronous methods

```csharp
var client = new Client();

// To use a proxy
var client = new Client(new WebProxy("169.57.1.84:8080"));
```

To get your IP
```csharp
// Sync
var ip = client.GetMyIPAddress(); // IPv4 by default
var ipv4 = client.GetMyIPAddress(System.Net.Sockets.AddressFamily.InterNetwork);
var ipv6 = client.GetMyIPAddress(System.Net.Sockets.AddressFamily.InterNetworkV6);
// Async
var ip = await client.GetMyIPAddressAsync();
var ipv4 = client.GetMyIPAddressAsync(System.Net.Sockets.AddressFamily.InterNetwork);
var ipv6 = client.GetMyIPAddressAsync(System.Net.Sockets.AddressFamily.InterNetworkV6);
```

[Lookup IP](https://whatismyipaddress.com/ip-lookup) Tool  
> This tool provides details about an IP address. It's estimated physical location (country, state, and city) and a map.
```csharp
var details = client.LookupIP(ip);
var details = await client.LookupIPAsync(ip);
```

[Advanced Proxy Check](https://whatismyipaddress.com/proxy-check) Tool  
> If you are using a proxy server use this tool to check and see if any information is being exposed. 
```csharp
var proxy = client.ProxyCheck(new WebProxy("138.68.240.218:8080")).IsProxyServer;
// sets timeout to 20 seconds and number of tries (try again if request fails) to 2 [Optional]
var proxy = client.ProxyCheck(new WebProxy("138.68.240.218:8080"), TimeSpan.FromSeconds(20000), 2).IsProxyServer;

var proxy = (await client.ProxyCheckAsync(new WebProxy("138.68.240.218:8080"))).IsProxyServer;
```

[Blacklist Check](https://whatismyipaddress.com/blacklist-check) Tool  
> This tool will check to see if your IP address is listed with more than 100 DNSbl's as a machine that mail should not be accepted from.
```csharp
var blacklist = client.BlacklistCheck(ip);
var blacklist = await client.BlacklistCheckAsync(ip);
var validityPercent = result.GoodPercent;
```

[IP To Hostname Lookup](https://whatismyipaddress.com/ip-hostname) Tool
> This tool provides the hostname of an IP address. (ie 192.168.1.1)
```csharp
var hostname = client.LookupHostname(ip);
var hostname = await client.LookupHostnameAsync(ip);
```

[Hostname to IP Lookup](https://whatismyipaddress.com/hostname-ip) Tool
> This tool provides the IP address of a hostname (ie www.yahoo.com)
```csharp
var resultIPs = client.LookupIPAddress("www.yahoo.com");
var resultIPs = await client.LookupIPAddressAsync("www.yahoo.com");
```

Full Test Example [Here](https://github.com/RyuzakiH/WhatIsMyIPAddress-API/blob/master/src/WhatIsMyIPAddress.Example/Program.cs)

# Supported Platforms
[.NET Standard 1.3](https://github.com/dotnet/standard/blob/master/docs/versions.md)

# Dependencies
* [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack)
