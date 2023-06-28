using ProgSieciowe.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using ProgSieciowe.Core.Enums;

namespace ProgSieciowe.Server
{
    internal class TcpServer
    {
        private static ILogger<TcpServer> _logger;
        private static string _directory;

        public static void TcpServerProcess(object? arg)
        {
            var (endPoint, loggerFactory, directory) = ((IPEndPoint, ILoggerFactory, string))arg!;
            _logger = loggerFactory.CreateLogger<TcpServer>();
            _directory = directory;

            var listener = new TcpListener(endPoint);
            listener.Start(10);

            _logger.LogInformation("Server started");

            while (true)
            {
                var client = listener.AcceptTcpClient();
                client.Client.ReceiveTimeout = Constants.DefaultTimeOut;
                client.Client.SendTimeout = Constants.DefaultTimeOut;

                var tcpCommunicator = new TcpCommunicator(client);
                HandleConnectionAsync(tcpCommunicator, loggerFactory);
            }
        }

        private static async void HandleConnectionAsync(ICommunicator communicator, ILoggerFactory loggerFactory)
        {
            var commandHandler = new ServerCommandHandler(communicator, loggerFactory, _directory);
            _logger.LogInformation("Client connected");

            var run = true;
            try
            {
                while (run)
                {
                    var msg = await communicator.ReceiveStringAsync(Constants.ClientRequestTimeOut);
                    _logger.LogInformation("Received command {msg}", msg);
                    var command = (CommandType)int.Parse(msg);
                    run = commandHandler.HandleCommand(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
            }

            _logger.LogInformation("Client disconnected");
        }
    }
}
