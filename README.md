# Microsoft.Extensions.Logging.Log4Net

Microsoft.Extensions.Logging.Log4Net

# Description

A couple of extension methods for adding log4net support to ASP.NET Core 1.1.

# Usage


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