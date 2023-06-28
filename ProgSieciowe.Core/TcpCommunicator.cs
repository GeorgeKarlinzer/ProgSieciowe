using System.Net.Sockets;
using System.Text;

namespace ProgSieciowe.Core
{
    public class TcpCommunicator : ICommunicator
    {
        private readonly TcpClient _tcpClient;

        public string RemoteEndPoint => _tcpClient.Client.RemoteEndPoint.ToString();

        public TcpCommunicator(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public void Send(string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            Send(bytes);
        }

        public void Send(byte[] bytes)
        {
            SendStream(new MemoryStream(bytes), bytes.Length);
        }

        public void SendFile(string path)
        {
            var fi = new FileInfo(path);
            var size = fi.Length;
            using var fstream = File.OpenRead(path);
            SendStream(fstream, size);
        }

        private void SendStream(Stream stream, long size)
        {
            _tcpClient.GetStream().Write(BitConverter.GetBytes(size), 0, 8);
            var maxBatchSize = 1024;
            var sent = 0L;

            while (sent < size)
            {
                var batchSize = size - sent > maxBatchSize ? maxBatchSize : (int)(size - sent);
                var buffer = new byte[batchSize];
                stream.Read(buffer, 0, batchSize);
                _tcpClient.GetStream().Write(buffer, 0, batchSize);
                sent += batchSize;
            }
        }

        public string ReceiveString()
        {
            return ReceiveStringAsync().Result;
        }

        public async Task<string> ReceiveStringAsync()
        {
            return await ReceiveStringAsync(Constants.DefaultTimeOut);
        }

        public void ReceiveFile(string path)
        {
            using var fstream = File.OpenWrite(path);
            ReceiveAllAsync(fstream, Constants.DefaultTimeOut).Wait();
        }

        public async Task<string> ReceiveStringAsync(int timeout)
        {
            var stream = new MemoryStream();
            await ReceiveAllAsync(stream, timeout);
            var bytes = stream.ToArray();
            var str = Encoding.UTF8.GetString(bytes);
            return str;
        }

        private async Task ReceiveAllAsync(Stream stream, int timeout)
        {
            var maxBatchSize = 1024;
            var buffer = new byte[8];
            await Task.Run(() =>
            {
                var task = _tcpClient.GetStream().ReadAsync(buffer).AsTask();
                task.Wait(timeout);
                if (task.IsCompleted)
                    return;
                throw new TimeoutException();
            });

            var size = BitConverter.ToInt64(buffer);

            var received = 0;

            while (received < size)
            {
                var batchSize = maxBatchSize > size - received ? (int)(size - received) : maxBatchSize;
                buffer = new byte[batchSize];
                await Task.Run(() =>
                {
                    var task = _tcpClient.GetStream().ReadAsync(buffer).AsTask();
                    task.Wait(timeout);
                    if (task.IsCompleted)
                        return;
                    throw new TimeoutException();
                });

                received += batchSize;
                stream.Write(buffer);
            }
        }
    }
}