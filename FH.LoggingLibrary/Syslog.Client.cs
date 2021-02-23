using System;
using System.Net;
using System.Net.Sockets;

namespace Syslog
{
    public enum Level
    {
        Emergency = 0,
        Alert = 1,
        Critical = 2,
        Error = 3,
        Warning = 4,
        Notice = 5,
        Information = 6,
        Debug = 7,
    }

    public enum Facility
    {
        Kernel = 0,
        User = 1,
        Mail = 2,
        Daemon = 3,
        Auth = 4,
        Syslog = 5,
        Lpr = 6,
        News = 7,
        UUCP = 8,
        Cron = 9,
        Local0 = 10,
        Local1 = 11,
        Local2 = 12,
        Local3 = 13,
        Local4 = 14,
        Local5 = 15,
        Local6 = 16,
        Local7 = 17,
    }

    public class Client: IDisposable
    {
        private IPAddress     _hostIp;
        private int _port;
        private bool          _useTCP;
        private UdpClient     _udpSocket;
        private TcpClient     _tcpSocket;
        private NetworkStream _stream;

        public Client(IPAddress server, int port, bool tcp = false)
        {
            this._hostIp = server;
            this._port = port;
            this._useTCP = tcp;
            if (_useTCP)
            {
                this._tcpSocket = new TcpClient();
                this._tcpSocket.Connect(_hostIp, _port);
                this._stream = _tcpSocket.GetStream();
                this._tcpSocket.LingerState = new LingerOption(true, 30);
            }
            else
            {
                this._udpSocket = new UdpClient();
                this._udpSocket.Connect(_hostIp, _port);
            }
        }

        public Client(IPAddress server, int port, Facility facility, Level level, string tag, bool tcp = false)
            : this(server, port, tcp)
        {
            this._defaultFacility = facility;
            this._defaultLevel = level;
            this._tag = tag;
        }
        private Facility _defaultFacility = Facility.Syslog;
        public Facility DefaultFacility { get; set; }

        public string _tag;
        public string Tag { get; set; }

        private Level _defaultLevel = Level.Warning;
        public Level DefaultLevel { get; set; }

        // Send() long form with enum
        public void Send(Syslog.Rfc3164Message message)
        {
            byte[] sendBytes = message.ToBytes();

            // TODO: change to use asynchronous sending?

            if (_useTCP)
            {
                bool success = false;
                do
                {
                    try
                    {
                        _stream.Write(sendBytes, 0, sendBytes.Length);
                        _stream.Flush();
                        success = _tcpSocket.Connected;
                    }
                    catch (SocketException e)
                    {
                        // 10035 == WSAEWOULDBLOCK i.e. already connected
                        if (e.NativeErrorCode != 10035)
                        {
                            //_tcpSocket.Connect(_hostIp, _port); // no, need to establish fresh new connection
                            if (_stream != null) _stream.Close();
                            if (_tcpSocket != null) _tcpSocket.Close();
                            _tcpSocket = new TcpClient();
                            this._tcpSocket.Connect(_hostIp, _port);
                            _stream = _tcpSocket.GetStream();
                            _tcpSocket.LingerState = new LingerOption(true, 30);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        if (_stream != null) _stream.Close();
                        if (_tcpSocket != null) _tcpSocket.Close();
                        _tcpSocket = new TcpClient();
                        this._tcpSocket.Connect(_hostIp, _port);
                        _stream = _tcpSocket.GetStream();
                        _tcpSocket.LingerState = new LingerOption(true, 30);
                    }
                } while (!success);
            }
            else
            {
                _udpSocket.SendAsync(sendBytes, sendBytes.Length).Wait();
            }
        }

        // Send() simplified overload uses previously set default facility & level
        public void Send(string text)
        {
            Send(new Rfc3164Message(_defaultFacility,
                _defaultLevel,
                _tag,
                text));
        }

        public void Close()
        {
            if (_useTCP)
            {
                _stream.Close();
                _tcpSocket.Close();
            }
            else
            {
                _udpSocket.Close();
            }
        }

        public void Dispose()
        {
            Close();   
        }

        // TODO: support callback for detailed logging?
    }

    public class DemoClient
    {
        public static void Main(string[] args)
        {

            using (Syslog.Client c = new Syslog.Client(IPAddress.Parse("127.0.0.1"), 514, Syslog.Facility.Local6, Syslog.Level.Warning, "Test.Application"))
            {
                c.Send("This is a test of the syslog client code.");
            }
        }
    }
}