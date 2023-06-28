using Microsoft.Extensions.Logging;
using ProgSieciowe.Core;
using ProgSieciowe.Core.Enums;

namespace ProgSieciowe.Server
{
    internal abstract class ServerBase
    {
        protected readonly ICommunicator _communicator;
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly string _directory;

        public ServerBase(ICommunicator communicator, ILogger logger, ILoggerFactory loggerFactory, string directory)
        {
            _communicator = communicator;
            _logger = logger;
            _loggerFactory = loggerFactory;
            _directory = directory;
        }

        public virtual async Task StartServerAsync()
        {
            var commandHandler = new ServerCommandHandler(_communicator, _loggerFactory, _directory);
            _logger.LogInformation($"Client {_communicator.RemoteEndPoint} connected");

            var run = true;
            try
            {
                while (run)
                {
                    var msg = await _communicator.ReceiveStringAsync(Constants.ClientRequestTimeOut);
                    _logger.LogInformation($"Received command {msg} from {_communicator.RemoteEndPoint}", msg);
                    var command = (CommandType)int.Parse(msg);
                    run = commandHandler.HandleCommand(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Criticla error for client {_communicator.RemoteEndPoint}");
            }

            _logger.LogInformation($"Client {_communicator.RemoteEndPoint} disconnected");
        }
    }
}
