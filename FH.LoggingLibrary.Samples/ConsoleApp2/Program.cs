using System;
using System.Net;
using FH.Logging;
using Microsoft.Extensions.Logging;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {

            // ILoggerFactory
            var loggerFactory = LoggerFactory.Create(builder => 
            {
                builder
                    .ClearProviders()
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFilter("Microsoft", LogLevel.Debug)
                    .AddFilter("System", LogLevel.Debug)
                    .AddFilter("ConsoleApp2.Program", LogLevel.Debug)
                    .AddConsole()
                    .AddSyslog(
                        new SysLogSettings() 
                        { 
                            Host = "172.17.82.26",
                            Port = 514,
                        });
            });

            var _logger = loggerFactory.CreateLogger("Program");

            _logger.LogInformation("LogInformation");
            _logger.LogDebug("LogDebug");
            _logger.LogTrace("LogTrace");
            _logger.LogError("LogError");
            _logger.LogCritical("LogCritical");
            _logger.LogWarning("LogWarning");
        }
    }
}
