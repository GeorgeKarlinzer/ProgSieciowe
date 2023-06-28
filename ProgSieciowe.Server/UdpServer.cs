using Microsoft.Extensions.Logging;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace ProgSieciowe.Server
{
    internal class UdpServer : ServerBase
    {
        private readonly IPEndPoint _endPoint;

        public event Action<IPEndPoint> Stop;

        public UdpServer(UdpClient client, IPEndPoint endPoint, Pipe pipe, ILoggerFactory loggerFactory, string directory)
            : base(new ServerUdpCommicator(client, endPoint, pipe), loggerFactory.CreateLogger<UdpServer>(), loggerFactory, directory)
        {
            _endPoint = endPoint;
        }

        public async override Task StartServerAsync()
        {
            await base.StartServerAsync();
            Stop?.Invoke(_endPoint);
        }
    }
}
