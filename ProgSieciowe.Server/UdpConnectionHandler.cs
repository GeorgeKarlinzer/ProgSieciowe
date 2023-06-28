using Microsoft.Extensions.Logging;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace ProgSieciowe.Server
{
    internal class UdpConnectionHandler : ConnectionHandlerBase
    {
        private readonly IPEndPoint _endPoint;

        public event Action<IPEndPoint> Stop;

        public UdpConnectionHandler(UdpClient client, IPEndPoint endPoint, Pipe pipe, ILoggerFactory loggerFactory, string directory)
            : base(new ServerUdpCommicator(client, endPoint, pipe), loggerFactory.CreateLogger<UdpConnectionHandler>(), loggerFactory, directory)
        {
            _endPoint = endPoint;
        }

        public async override Task StartAsync()
        {
            await base.StartAsync();
            Stop?.Invoke(_endPoint);
        }
    }
}
