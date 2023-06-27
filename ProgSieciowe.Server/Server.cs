using Microsoft.Extensions.Logging;
using ProgSieciowe.Core;
using ProgSieciowe.Core.Enums;
using System.Net;
using System.Net.Sockets;

namespace ProgSieciowe.Server
{
    public class Server
    {
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly string _directory;
        private readonly ILogger<Server> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public Server(IPAddress address, int port, string directory, ILoggerFactory loggerFactory)
        {
            _address = address;
            _port = port;
            _directory = directory;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Server>();
        }

        public void Start()
        {
            try
            {
                var arg = (new IPEndPoint(_address, _port), _loggerFactory, _directory);

                var udpThread = new Thread(new ParameterizedThreadStart(UdpServer.UpdServerProcess))
                {
                    IsBackground = true,
                    Name = "UDP server thread"
                };
                udpThread.Start(arg);

                var tcpThread = new Thread(new ParameterizedThreadStart(TcpServer.TcpServerProcess))
                {
                    IsBackground = true,
                    Name = "TCP server thread"
                };
                tcpThread.Start(arg);

                Console.WriteLine("Press <ENTER> to stop the servers.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main exception: " + ex);
            }
        }
    }
}