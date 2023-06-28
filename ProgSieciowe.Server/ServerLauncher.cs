using Microsoft.Extensions.Logging;
using ProgSieciowe.Core;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace ProgSieciowe.Server
{
    public class ServerLauncher
    {
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly string _directory;
        private readonly ILogger<ServerLauncher> _logger;
        private readonly ILoggerFactory _loggerFactory;


        public ServerLauncher(IPAddress address, int port, string directory, ILoggerFactory loggerFactory)
        {
            _address = address;
            _port = port;
            _directory = directory;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ServerLauncher>();
        }

        public void Launch()
        {
            try
            {
                _logger.LogInformation("Launcher started");
                var arg = (new IPEndPoint(_address, _port), _loggerFactory, _directory);

                var udpThread = new Thread(new ParameterizedThreadStart(UpdServerProcess))
                {
                    IsBackground = true,
                    Name = "UDP server thread"
                };
                udpThread.Start(arg);

                var tcpThread = new Thread(new ParameterizedThreadStart(TcpServerProcess))
                {
                    IsBackground = true,
                    Name = "TCP server thread"
                };
                tcpThread.Start(arg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Launcher error");
            }
        }

        private static void TcpServerProcess(object? arg)
        {
            var (endPoint, loggerFactory, directory) = ((IPEndPoint, ILoggerFactory, string))arg!;
            var logger = loggerFactory.CreateLogger<TcpServer>();

            var listener = new TcpListener(endPoint);
            listener.Start(Constants.MaxConnections);

            logger.LogInformation("Tcp server started");

            while (true)
            {
                var client = listener.AcceptTcpClient();
                client.Client.ReceiveTimeout = Constants.DefaultTimeOut;
                client.Client.SendTimeout = Constants.DefaultTimeOut;

                var tcpServer = new TcpServer(client, loggerFactory, directory);
                _ = tcpServer.StartServerAsync();
            }
        }

        private static void UpdServerProcess(object? arg)
        {
            var (endPoint, loggerFactory, directory) = ((IPEndPoint, ILoggerFactory, string))arg!;
            var logger = loggerFactory.CreateLogger<UdpServer>();
            var connections = new ConcurrentDictionary<string, Pipe>();
            var udpServer = new UdpClient(endPoint);

            udpServer.Client.ReceiveTimeout = Constants.DefaultTimeOut;
            udpServer.Client.SendTimeout = Constants.DefaultTimeOut;

            logger.LogInformation("Udp server started");

            while (true)
            {
                var result = udpServer.ReceiveAsync().Result;

                if (!connections.ContainsKey(result.RemoteEndPoint.ToString()))
                {
                    if (connections.Count == Constants.MaxConnections)
                        continue;

                    var p = new Pipe();
                    connections.TryAdd(result.RemoteEndPoint.ToString(), p);
                    var udpClientServer = new UdpServer(udpServer, result.RemoteEndPoint, p, loggerFactory, directory);
                    udpClientServer.Stop += remoteEndPoint => connections.Remove(remoteEndPoint.ToString(), out _);
                    _ = udpClientServer.StartServerAsync();
                }
                else
                {
                    connections.TryGetValue(result.RemoteEndPoint.ToString(), out var p);
                    _ = p!.Writer.WriteAsync(result.Buffer);
                }
            }
        }
    }
}