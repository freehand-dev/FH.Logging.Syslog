using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text;

namespace FH.Logging
{
    // loggerFactory.AddSyslog("192.168.210.56", 514);
    public static class SyslogLoggerProviderExtensions
    {

        /*
            "IncludeScopes": false,
            "LogLevel": {
               "Default": "Warning",
               "Microsoft": "Error",
            }
            "Syslog":
            {
               "Host": "127.0.0.1",
               "Port": 514
            }
        */

        private static SyslogLoggerProvider CreateSyslogLoggerProvider(SysLogSettings settings)
        {
            SyslogLoggerProvider provider = new SyslogLoggerProvider(settings);
            return provider;
        }

        private static SyslogLoggerProvider CreateSyslogLoggerProvider(string host, int port, Func<string, LogLevel, bool> filter = null)
        {
            SyslogLoggerProvider provider = SyslogLoggerProviderExtensions.CreateSyslogLoggerProvider(
                                                IPAddress.Parse(host ?? "127.0.0.1"),
                                                port,
                                                filter);
            return provider;
        }

        private static SyslogLoggerProvider CreateSyslogLoggerProvider(IPAddress host, int port, Func<string, LogLevel, bool> filter = null)
        {
            SyslogLoggerProvider provider = new SyslogLoggerProvider(
                                                host,
                                                port,
                                                filter);
            return provider;
        }

        public static ILoggerFactory AddSyslog(this ILoggerFactory factory, IPAddress host, int port, Func<string, LogLevel, bool> filter = null)
        {
            factory?.AddProvider(
                SyslogLoggerProviderExtensions.CreateSyslogLoggerProvider(host, port, filter));
            return factory;
        }

        public static ILoggingBuilder AddSyslog(this ILoggingBuilder loggingBuilder, IPAddress host, int port, Func<string, LogLevel, bool> filter = null)
        {
            loggingBuilder?.AddProvider(
                SyslogLoggerProviderExtensions.CreateSyslogLoggerProvider(host, port, filter));
            return loggingBuilder;
        }

        public static ILoggingBuilder AddSyslog(this ILoggingBuilder loggingBuilder, SysLogSettings settings)
        {
            loggingBuilder?.AddProvider(
                SyslogLoggerProviderExtensions.CreateSyslogLoggerProvider(settings)
            );
            return loggingBuilder;
        }

        
    }
}
