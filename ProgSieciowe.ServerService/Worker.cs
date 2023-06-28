using System.Net;

namespace ProgSieciowe.ServerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Config _config;
        private readonly ILoggerFactory _loggerFactory;

        public Worker(Config config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Worker>();
            _config = config;
            _loggerFactory = loggerFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var address = IPAddress.Parse(_config.IpAddress);
            var port = int.Parse(_config.Port);
            var directory = _config.WorkingDirectory;
            var server = new Server.ServerLauncher(address, port, directory, _loggerFactory);

            server.Launch();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}