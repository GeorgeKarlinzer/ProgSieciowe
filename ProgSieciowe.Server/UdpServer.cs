using Microsoft.Extensions.Logging;
using ProgSieciowe.Core;
using ProgSieciowe.Core.Enums;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

            udpServer.Client.ReceiveTimeout = Constants.DefaultTimeOut;
            udpServer.Client.SendTimeout = Constants.DefaultTimeOut;

            _logger = loggerFactory.CreateLogger<UdpServer>();
            _directory = directory;

            while (true)
            {
                var result = udpServer.ReceiveAsync().Result;

                if (!_connections.ContainsKey(result.RemoteEndPoint.ToString()))
                {
                    if (_connections.Count == Constants.MaxConnections)
                        continue;

                    var p = new Pipe();
                    _connections.TryAdd(result.RemoteEndPoint.ToString(), p);
                    var udpClientServer = new UdpServer(udpServer, result.RemoteEndPoint, p);
                    udpClientServer.StartAsync(loggerFactory);
                }
                else
                {
                    _connections.TryGetValue(result.RemoteEndPoint.ToString(), out var p);
                    p.Writer.WriteAsync(result.Buffer);
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
            var commandHandler = new ServerCommandHandler(this, loggerFactory, _directory);
            _logger.LogInformation("Client connected");

            var run = true;
            try
            {
                while (run)
                {
                    var msg = await ReceiveStringAsync(Constants.ClientRequestTimeOut);
                    _logger.LogInformation("Received command {msg}", msg);
                    var command = (CommandType)int.Parse(msg);
                    run = commandHandler.HandleCommand(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
            }

            _connections.Remove(_endPoint.ToString(), out _);
            _logger.LogInformation("Client disconnected");
        }

        protected override async Task<byte[]> InternalReceiveAsync(int timeout)
        {
            var result = await Task.Run(() =>
            {
                var task = _pipe.Reader.ReadAsync().AsTask();
                task.Wait(timeout);
                if (task.IsCompleted)
                {
                    var buffer = task.Result.Buffer.FirstSpan.ToArray();
                    _pipe.Reader.AdvanceTo(task.Result.Buffer.End);
                    return buffer;
                }

                throw new TimeoutException();
            });

            return result;
        }
    }
}
