using log4net;
using log4net.Appender;
using log4net.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Log4Net;

// Exercise the netstandard2.0 library on a modern runtime.
// This does not exercise the ASP.NET Core sample; run the Sample project for that.
var directory = Path.Combine(Path.GetTempPath(), "log4net-security-" + Guid.NewGuid());
Directory.CreateDirectory(directory);
var repository = LogManager.CreateRepository(Guid.NewGuid().ToString());
try
{
    var config = Path.Combine(directory, "log4net.xml");
    const string appender = "<appender name=\"Memory\" type=\"log4net.Appender.MemoryAppender\" />";
    const string root = "<root><level value=\"ALL\" /><appender-ref ref=\"Memory\" /></root>";
    File.WriteAllText(config, "<log4net>" + appender + root + "</log4net>");
    XmlConfigurator.Configure(repository, new FileInfo(config));
    var memory = repository.GetAppenders().OfType<MemoryAppender>().Single();
    using (var provider = new Log4NetProvider(repository.Name))
    {
        provider.CreateLogger("Smoke").LogInformation("security-smoke");
    }
    if (memory.GetEvents().Single().RenderedMessage != "security-smoke")
        throw new Exception("Normal configuration/logging failed.");
    Console.WriteLine("PASS: normal XML configuration and provider logging");

    repository.ResetConfiguration();
    var external = Path.Combine(directory, "external.xml");
    File.WriteAllText(external, appender);
    File.WriteAllText(config,
        "<!DOCTYPE log4net [<!ENTITY external SYSTEM \"" + new Uri(external).AbsoluteUri +
        "\">]><log4net>&external;" + root + "</log4net>");
    XmlConfigurator.Configure(repository, new FileInfo(config));
    if (repository.GetAppenders().Length != 0)
        throw new Exception("XXE regression: configuration loaded an external appender.");
    Console.WriteLine("PASS: external XML entity was not loaded");
}
finally
{
    repository.Shutdown();
    Directory.Delete(directory, recursive: true);
}
