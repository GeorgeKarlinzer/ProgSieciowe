using System;

namespace ProgSieciowe.Core
{
    public interface ICommunicator
    {
        void Send(string msg);
        void Send(byte[] bytes);
        string ReceiveString();
        Task<string> ReceiveStringAsync();
        void SendFile(string path);
        void ReceiveFile(string path);
    }
}