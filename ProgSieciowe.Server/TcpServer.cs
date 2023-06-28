using Microsoft.Extensions.Logging;
using ProgSieciowe.Core;
using System.Net.Sockets;

namespace ProgSieciowe.Server
{
    internal class TcpServer : ServerBase
    {
        public TcpServer(TcpClient tcpClient, ILoggerFactory loggerFactory, string directory)
            : base(new TcpCommunicator(tcpClient), loggerFactory.CreateLogger<TcpServer>(), loggerFactory, directory)
        {
        }
    }
}
