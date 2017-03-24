using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Microsoft.Extensions.Logging.Log4Net
{
    public static class Log4NetAspExtensions
    {
        public static void ConfigureLog4Net(this IHostingEnvironment appEnv, IConfigurationSection config)
        {
            GlobalContext.Properties["appRoot"] = appEnv.ContentRootPath;
            XmlConfigurator.Configure(LogManager.CreateRepository(config["Repository"]), new FileInfo(Path.Combine(appEnv.ContentRootPath, config["ConfigFileRelativePath"])));
        }
        public static void ConfigureLog4Net(string currentDir, IConfigurationSection config)
        {
            GlobalContext.Properties["appRoot"] = currentDir;
            XmlConfigurator.Configure(LogManager.CreateRepository(config["Repository"]), new FileInfo(Path.Combine(currentDir, config["ConfigFileRelativePath"])));
        }
        public static void AddLog4Net(this ILoggerFactory loggerFactory, IConfigurationSection config)
        {
            loggerFactory.AddProvider(new Log4NetProvider(config["Repository"]));
        }
    }
}