# WhatIsMyIPAddress-API
Unofficial API for [WhatIsMyIPAddress](https://whatismyipaddress.com) in .NET Standard


# Usage

[Lookup IP](https://whatismyipaddress.com/ip-lookup) tool
```csharp
WhatIsMyIPAddress.LookupIP("103.234.220.197");
```

[Proxy Check](https://whatismyipaddress.com/proxy-check) Tool
```csharp
var checkResults = WhatIsMyIPAddress.ProxyCheck(new WebProxy("139.59.99.234", 8080), 60000);
// To get final result
var result = checkResults.IsProxyServer;
```

[Proxy Check](https://whatismyipaddress.com/proxy-check) Tool Async
```csharp
// 2 is the maximum number of tries if request failed (number to try again times)
var checkResults = await WhatIsMyIPAddress.ProxyCheckAsync(new WebProxy("139.59.99.234", 8080), 20000, 3);
var result = checkResults.IsProxyServer;
```

# Dependencies
* [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack)
