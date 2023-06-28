namespace ProgSieciowe.Core
{
    public interface ICommunicator
    {
        string RemoteEndPoint { get; }
        void Send(string msg);
        void Send(byte[] bytes);
        string ReceiveString();
        Task<string> ReceiveStringAsync();
        Task<string> ReceiveStringAsync(int timeout);
        void SendFile(string path);
        void ReceiveFile(string path);
    }
}