using System;
using System.Net;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Sending test message to localhost Syslog server...");

            using (Syslog.Client c = new Syslog.Client(IPAddress.Parse("172.17.82.26"), 514, Syslog.Facility.Local6, Syslog.Level.Warning, "Test.Application Test"))
            {
                c.Send("This is a test of the syslog client code. Привет");

                c.Send(new Syslog.Rfc3164Message(Syslog.Facility.Local6, Syslog.Level.Warning, "Test.Application test", "This is a test of the syslog client code. Привет")
                {
                    PID = 1000
                });
            }

            Console.WriteLine("Ready...");
            Console.ReadKey();
        }
    }
}
