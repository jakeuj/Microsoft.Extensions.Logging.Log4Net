# Microsoft.Extensions.Logging.Log4Net

> [!WARNING]
> **Legacy reference:** this project targets ASP.NET Core 1.1 and is not
> presented as a currently maintained production package. Use it as historical
> implementation material and evaluate supported logging integrations for new
> applications.

## Description

A couple of extension methods for adding log4net support to ASP.NET Core 1.1.

## Security maintenance

The source now references log4net 2.0.17 to fix Dependabot alert #4
([CVE-2018-1285](https://github.com/advisories/GHSA-2cwj-8chv-9pp9), XML external
entity processing). The sample references the local project so it uses this fix
instead of the previously published 1.0.0 package.

This is a compatibility-preserving fix for the legacy .NET Core 1.1 target, not
a complete security upgrade. Log4net 2.0.17 is still affected by alert #6
([GHSA-4f7c-pmjv-c25w](https://github.com/advisories/GHSA-4f7c-pmjv-c25w)); fixing
that requires log4net 3.3.0 or later and a framework migration. The old ASP.NET
Core MVC and runtime dependencies also retain known vulnerabilities. Previously
published NuGet packages are unchanged.

Validation (requires the .NET 10 SDK for the smoke test):

```sh
dotnet build Microsoft.Extensions.Logging.Log4Net.sln -c Release
dotnet run --project tests/SecuritySmoke -c Release
dotnet list Microsoft.Extensions.Logging.Log4Net.sln package --vulnerable --include-transitive
```

The smoke test checks normal XML configuration/provider logging and verifies
that an external XML entity cannot inject an appender. It runs the built library
on .NET 10; it does not validate the sample on a .NET Core 1.1 runtime. Modern
XML parser defaults also reject this payload with log4net 2.0.8, so this smoke
test alone does not reproduce the historical vulnerability. The NuGet audit
confirms removal of GHSA-2cwj-8chv-9pp9 from both projects' dependency graphs.

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
    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)

            .AddEnvironmentVariables();

        Configuration = builder.Build();

        //  Configure log4net
        env.ConfigureLog4Net(Configuration.GetSection("Log4Net"));
        // ...
    }
    // ...
}
```

## 4. Register provider with ILoggerFactory

Make a call to `loggerFactory.AddLog4Net` inside of the `Configure` method in `Startup.cs`.

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    loggerFactory.MinimumLevel = LogLevel.Verbose;
    loggerFactory.AddConsole();
    loggerFactory.AddDebug();

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
