# Microsoft.Extensions.Logging.Log4Net

> [!WARNING]
> **Legacy reference:** this project started as an ASP.NET Core 1.1 integration
> and is not presented as a currently maintained production package. Use it as
> historical implementation material and evaluate supported logging integrations
> for new applications.

## Description

A couple of extension methods for adding log4net support to ASP.NET Core via
`Microsoft.Extensions.Logging`. The library targets .NET Standard 2.0 and the
sample runs on .NET 10.

## Security maintenance

| Dependabot alert | Advisory | Fix |
| --- | --- | --- |
| #4 | [GHSA-2cwj-8chv-9pp9](https://github.com/advisories/GHSA-2cwj-8chv-9pp9) (log4net XXE) | log4net 2.0.10+ (first fixed in 2.0.17 here) |
| #6 | [GHSA-4f7c-pmjv-c25w](https://github.com/advisories/GHSA-4f7c-pmjv-c25w) (log4net XmlLayout silent event loss) | log4net 3.3.0+ (now 3.4.0) |
| #1, #2, #3, #5 | `Microsoft.AspNetCore.Mvc` 1.1.x advisories in the sample | Sample moved to .NET 10; MVC now comes from the shared framework |

Log4net 3.x only ships `netstandard2.0` / `net462` assemblies, so closing alert
#6 required retargeting the library from `netcoreapp1.1` to `netstandard2.0`.
The `Microsoft.AspNetCore.Hosting.Abstractions` reference moved to the serviced
2.3.x line and the `Microsoft.Extensions.*` abstractions to 8.0.x, which keeps
the transitive dependency graph free of known advisories. The package version is
bumped to 2.0.0 because .NET Core 1.1 consumers can no longer use it; the
previously published 1.0.0 package on NuGet is unchanged.

Validation (requires the .NET 10 SDK):

```sh
dotnet build Microsoft.Extensions.Logging.Log4Net.sln -c Release
dotnet run --project tests/SecuritySmoke -c Release
dotnet list Microsoft.Extensions.Logging.Log4Net.sln package --vulnerable --include-transitive
```

The smoke test checks normal XML configuration/provider logging and verifies
that an external XML entity cannot inject an appender. The NuGet audit reports
no vulnerable direct or transitive packages for either project.

## Usage

## 1. appsettings.json

```json
{
  "Log4Net": {
    "ConfigFileRelativePath": "log4net.xml",
    "Repository": "NETCoreRepository"
  },
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

## 2. log4net.xml


```xml
<?xml version="1.0" encoding="utf-8" ?>
<log4net>
  <appender name="RollingFile" type="log4net.Appender.FileAppender">
    <file type="log4net.Util.PatternString" value="%property{appRoot}\app.log" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%-5p %d{hh:mm:ss} %message%newline" />
    </layout>
  </appender>

  <root>
    <level value="DEBUG" />
    <appender-ref ref="RollingFile" />
  </root>
</log4net>
```


## 3. Configure log4net at Startup

Add an extra line in the `Startup.cs` constructor to tell it where to find the log4net XML file:

```csharp
public class Startup
{
    public Startup(IWebHostEnvironment env, IConfiguration configuration)
    {
        Configuration = configuration;

        //  Configure log4net
        Log4NetAspExtensions.ConfigureLog4Net(env.ContentRootPath, Configuration.GetSection("Log4Net"));
        // ...
    }
    // ...
}
```

On ASP.NET Core 2.x hosts the `IHostingEnvironment` extension form is still available:
`env.ConfigureLog4Net(Configuration.GetSection("Log4Net"))`.

## 4. Register provider with ILoggerFactory

Make a call to `loggerFactory.AddLog4Net` inside of the `Configure` method in `Startup.cs`.

```csharp
public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
{
    // Register Log4Net
    loggerFactory.AddLog4Net(Configuration.GetSection("Log4Net"));
    // ...
}
```

## 5. Done

Now you will be able to use the Microsoft Logging framework throughout your application, and log4net will be used as a logging provider (based on the configuration provided in `log4net.xml`).

```csharp
public class HomeController : Controller
{
    private readonly ILogger _logger;
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    public void Index()
    {
        _logger.LogInformation("This will get written to app.log via log4net.");
    }
}
```
