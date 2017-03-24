using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Log4Net
{
    public class Log4NetProvider : ILoggerProvider
    {
        private readonly string _repository;
        private IDictionary<string, ILogger> _loggers = new Dictionary<string, ILogger>();
        public Log4NetProvider(string repository)
        {
            _repository = repository;
        }
        public ILogger CreateLogger(string name)
        {
            if (!_loggers.ContainsKey(name))
            {
                lock (_loggers)
                {
                    // Have to check again since another thread may have gotten the lock first
                    if (!_loggers.ContainsKey(name))
                    {
                        _loggers[name] = new Log4NetAdapter(_repository, name);
                    }
                }
            }
            return _loggers[name];
        }
        public void Dispose()
        {
            _loggers.Clear();
            _loggers = null;
        }
    }
}