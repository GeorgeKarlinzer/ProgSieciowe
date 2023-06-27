using Microsoft.Extensions.Logging;
using ProgSieciowe.Core;
using ProgSieciowe.Core.Enums;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace ProgSieciowe.Server
{
    internal class UdpServer : UdpCommunicator
    {
        private static readonly ConcurrentDictionary<string, Pipe> _connections = new();
        private static ILogger<UdpServer> _logger;
        private static string _directory;

        public static void UpdServerProcess(object? arg)
        {
            var (endPoint, loggerFactory, directory) = ((IPEndPoint, ILoggerFactory, string))arg!;
            var udpServer = new UdpClient(endPoint);
            _logger = loggerFactory.CreateLogger<UdpServer>();
            _directory = directory;

            while (true)
            {
                var result = udpServer.ReceiveAsync().Result;

                if (_connections.ContainsKey(result.RemoteEndPoint.ToString()))
                {
                    _connections.TryGetValue(result.RemoteEndPoint.ToString(), out var p);
                    p.Writer.WriteAsync(result.Buffer).AsTask().Wait();
                }
                else
                {
                    var p = new Pipe();
                    _connections.TryAdd(result.RemoteEndPoint.ToString(), p);
                    var udpClientServer = new UdpServer(udpServer, result.RemoteEndPoint, p);
                    udpClientServer.StartAsync(loggerFactory);
                }
            }
        }

        private readonly Pipe _pipe;

        public UdpServer(UdpClient client, IPEndPoint endPoint, Pipe pipe) : base(client, endPoint)
        {
            _pipe = pipe;
        }

        private async void StartAsync(ILoggerFactory loggerFactory)
        {
            var commandHandler = new CommandHandler(this, loggerFactory, _directory);
            var run = true;
            while (run)
            {
                var msg = await ReceiveStringAsync();
                _logger.LogInformation("Received command {msg}", msg);
                var command = (CommandType)int.Parse(msg);
                run = commandHandler.HandleCommand(command);
            }
            _connections.Remove(_endPoint.ToString(), out _);
        }

        protected override async Task<byte[]> InternalReceiveAsync()
        {
            var result = await _pipe.Reader.ReadAsync();
            var buffer = result.Buffer.FirstSpan.ToArray();
            _pipe.Reader.AdvanceTo(result.Buffer.End);
            return buffer;
        }
    }
}
