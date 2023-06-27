using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProgSieciowe.Core
{
    public class UdpCommunicator : ICommunicator
    {
        protected UdpClient _client;
        protected IPEndPoint _endPoint;

        public UdpCommunicator(UdpClient client, IPEndPoint endPoint)
        {
            _client = client;
            _endPoint = endPoint;
        }

        protected virtual async Task<byte[]> InternalReceiveAsync()
        {
            while (true)
            {
                var result = await _client.ReceiveAsync();
                if (result.RemoteEndPoint.ToString() == _endPoint.ToString())
                    return result.Buffer;
            }
        }

        public void ReceiveFile(string path)
        {
            var received = 0;
            var size = int.Parse(ReceiveString());
            Send("1");
            using var file = File.OpenWrite(path);

            while (received < size)
            {
                var buffer = InternalReceiveAsync().Result;
                Send("1");
                file.Write(buffer, 0, buffer.Length);
                received += buffer.Length;
            }
        }

        public void SendFile(string path)
        {
            var maxBatchSize = 65507;
            var sent = 0;

            var fi = new FileInfo(path);
            var size = fi.Length;

            Send(Encoding.UTF8.GetBytes(size.ToString()));
            if (ReceiveString() != "1")
                return;

            using var file = File.OpenRead(path);

            while (sent < size)
            {
                var remain = size - file.Position;
                var batchSize = remain < maxBatchSize ? (int)remain : maxBatchSize;
                var buffer = new byte[batchSize];
                file.Read(buffer, 0, batchSize);
                _client.Send(buffer, batchSize, _endPoint);
                sent += batchSize;
                if (ReceiveString() != "1")
                    return;
            }
        }

        public string ReceiveString()
        {
            return ReceiveStringAsync().Result;
        }

        public async Task<string> ReceiveStringAsync()
        {
            var buffer = await InternalReceiveAsync();
            return Encoding.UTF8.GetString(buffer);
        }

        public void Send(string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            Send(bytes);
        }

        public void Send(byte[] bytes)
        {
            _client.Send(bytes, _endPoint);
        }
    }
}