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
// Last parameter value setting 3 as the maximum number of tries if request failed (number to try again times)
var checkResults = await WhatIsMyIPAddress.ProxyCheckAsync(new WebProxy("139.59.99.234", 8080), 20000, 3);
var result = checkResults.IsProxyServer;
```

[Blacklist Check](https://whatismyipaddress.com/blacklist-check) Tool
```csharp
var result = WhatIsMyIPAddress.BlacklistCheck("139.59.99.234");
var validityPercent = result.GoodPercent;
```


# Dependencies
* [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack)
