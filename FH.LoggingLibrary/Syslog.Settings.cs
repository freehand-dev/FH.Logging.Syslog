using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FH.Logging
{
    //
    // Summary:
    //     Settings for Microsoft.Extensions.Logging.EventLog.EventLogLogger.
    public class SysLogSettings
    {
        //
        // Summary:
        //     
        public string Host { get; set; }

        //
        // Summary:
        //     
        public int Port { get; set; }

        //
        // Summary:
        //     The function used to filter events based on the log level.
        public Func<string, LogLevel, bool> Filter { get; set; }
    }
}
