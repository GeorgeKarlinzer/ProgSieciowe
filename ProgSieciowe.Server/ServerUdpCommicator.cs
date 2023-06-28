using ProgSieciowe.Core;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProgSieciowe.Server
{
    internal class ServerUdpCommicator : UdpCommunicator
    {
        private readonly Pipe _pipe;

        public ServerUdpCommicator(UdpClient client, IPEndPoint endPoint, Pipe pipe) : base(client, endPoint)
        {
            _pipe = pipe;
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
