using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace FH.Logging
{
    public class SyslogLoggerProvider: ILoggerProvider
    {
        private IPAddress _host;
        private int _port;
    
        private readonly Func<string, LogLevel, bool> _filter;



        
        public SyslogLoggerProvider(SysLogSettings settings)
        {
            _host = (settings.Host == null ) ? IPAddress.Parse("127.0.0.1") : IPAddress.Parse(settings.Host);
            _port = (settings.Port <= 0) ? 514 : settings.Port;
            _filter = settings.Filter;
        }

        public SyslogLoggerProvider(IPAddress host,
                                    int port,
                                    Func<string, LogLevel, bool> filter)
        {
            _host = host ?? IPAddress.Parse("127.0.0.1");
            _port = (port <= 0) ? 514 : port;
            _filter = filter;
        }
    
        public ILogger CreateLogger(string categoryName)
        {
            return new SyslogLogger(categoryName, _host, _port, _filter);
        }
    
        public void Dispose()
        {
        }
    }
    
}
