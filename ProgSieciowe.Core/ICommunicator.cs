using System.Net.Sockets;
using System.Text;

namespace ProgSieciowe.Core
{
    public interface ICommunicator
    {
        void Send(string msg);
        void Send(byte[] bytes);
        void SendFile(string path);
        string ReceiveString();
        IEnumerable<byte[]> ReceiveBatchCollection(int maxBatchSize = 1024);
    }

    public class TcpCommunicator : ICommunicator
    {
        private readonly Socket _socket;

        public TcpCommunicator(Socket socket)
        {
            _socket = socket;
        }

        public IEnumerable<byte[]> ReceiveBatchCollection(int maxBatchSize = 1024)
        {
            var buffer = new byte[4];
            _ = _socket.ReceiveAsync(buffer).Result;

            var size = BitConverter.ToInt32(buffer);
            var received = 0;

            while(received < size)
            {
                var batchSize = maxBatchSize > size - received ? size - received : maxBatchSize;
                buffer = new byte[batchSize];
                _ = _socket.ReceiveAsync(buffer).Result;
                received += batchSize;
                yield return buffer.ToArray();
            }
        }

        public string ReceiveString()
        {
            return string.Join("", ReceiveBatchCollection().Select(Encoding.UTF8.GetString));
        }

        public void Send(string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            Send(bytes);
        }

        public void Send(byte[] bytes)
        {
            _socket.Send(BitConverter.GetBytes(bytes.Length));
            _socket.Send(bytes);
        }

        public void SendFile(string path)
        {
            var fi = new FileInfo(path);
            _socket.Send(BitConverter.GetBytes(fi.Length), 4, SocketFlags.None);
            _socket.SendFile(path);
        }
    }

    //public class UdpCommunicator : ICommunicator
    //{
    //}
}