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

        public async Task StartAsync()
        {
            var tcpListener = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tcpListener.Bind(new IPEndPoint(_address, _port));

            tcpListener.Listen(10);

            _logger.LogInformation("Server started");

            while (true)
            {
                var client = await tcpListener.AcceptAsync();
                HandleConnectionAsync(client);
            }
        }

        private async void HandleConnectionAsync(Socket client)
        {
            _logger.LogInformation("Client connected");

            var communicator = new TcpCommunicator(client);
            var commandHandler = new CommandHandler(communicator, _loggerFactory, _directory);

            var run = true;
            try
            {
                while (run)
                {
                    var msg = communicator.ReceiveString();
                    _logger.LogInformation("Received command {msg}", msg);
                    var command = (CommandType)int.Parse(msg);
                    run = commandHandler.HandleCommand(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
            }

            try
            {
                client.Close();
            }
            catch { }
            _logger.LogInformation("Client disconnected");
        }
    }
}