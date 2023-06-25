using System.Net.Sockets;
using System.Text;

namespace ProgSieciowe.Core
{
    public interface ICommunicator
    {
        void Send(string msg);
        void Send(byte[] bytes);
        Task<string> ReceiveAsync();
    }

    public class TcpCommunicator : ICommunicator
    {
        private readonly Socket _socket;

        public TcpCommunicator(Socket socket)
        {
            _socket = socket;
        }

        public async Task<string> ReceiveAsync()
        {
            var buffer = new byte[1024];
            await _socket.ReceiveAsync(buffer);
            var msg = Encoding.UTF8.GetString(buffer);
            msg = msg.TrimEnd('\0');
            return msg;
        }

        public void Send(string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);

            Send(bytes);
        }

        public void Send(byte[] bytes)
        {
            _socket.Send(bytes);
        }
    }

    //public class UdpCommunicator : ICommunicator
    //{
    //}
}