using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Syslog;

namespace FH.Logging
{
    public static class LogLevelHelper
    {
        public static Syslog.Level GetSyslogLogLevel<T>(T value) where T : struct, IConvertible
        {
            Type t = typeof(T);
            if (!t.IsEnum)
            {
                throw new ArgumentException("T must be an enum type.");
            }

            switch (value)
            {
                case LogLevel.Critical: 
                    return Syslog.Level.Critical;
                case LogLevel.Debug: 
                    return Syslog.Level.Debug;
                case LogLevel.Error: 
                    return Syslog.Level.Error;
                case LogLevel.Information: 
                case LogLevel.None: 
                case LogLevel.Trace:
                    return Syslog.Level.Information;
                case LogLevel.Warning:
                    return Syslog.Level.Warning;                
                default: 
                    return Syslog.Level.Debug;
            }
        }
    }
    public class SyslogLogger : ILogger
    {
        private readonly Syslog.Facility _facility = Syslog.Facility.Local6;
    
        private string _categoryName;
        private IPAddress _host;
        private int _port;
    
        private readonly Func<string, LogLevel, bool> _filter;
    
        public SyslogLogger(string categoryName,
                            IPAddress host,
                            int port,
                            Func<string, LogLevel, bool> filter)
        {
            _categoryName = categoryName;
            _host = host;
            _port = port;
    
            _filter = filter;
        }
    
        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }
    
        public bool IsEnabled(LogLevel logLevel)
        {
            return (_filter == null || _filter(_categoryName, logLevel));
        }
    
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
    
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
    
            var message = formatter(state, exception);
    
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
    
            message = $"{ logLevel }: {message}";
    
            if (exception != null)
            {
                message += Environment.NewLine + Environment.NewLine + exception.ToString();
            }
    
            var level = LogLevelHelper.GetSyslogLogLevel<LogLevel>(logLevel);

            using (Syslog.Client c = new Syslog.Client(this._host, this._port, this._facility, level, _categoryName))
            {
                c.Send(message);
            }

        }  
    }

    public class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance = new NoopDisposable();
    
        public void Dispose()
        {
        }
    }

}