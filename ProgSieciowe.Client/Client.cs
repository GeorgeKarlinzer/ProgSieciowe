using Microsoft.Extensions.Logging;
using ProgSieciowe.Core;
using ProgSieciowe.Core.Enums;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ProgSieciowe.Client
{
    public class Client
    {
        private readonly Protocol _protocol;
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly IInputOutput _io;

        public Client(Protocol protocol, IPAddress address, int port, IInputOutput io)
        {
            _protocol = protocol;
            _address = address;
            _port = port;
            _io = io;
        }

        public void Start()
        {
            var socket = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(_address, _port));
            _io.WriteString("Connection established");

            var communicator = new TcpCommunicator(socket);
            var commandHelper = new CommandHandler(communicator, _io);

            var run = true;
            try
            {
                while (run)
                {
                    _io.WriteString("Enter command:");
                    var input = _io.GetString();
                    var args = new List<string>();

                    foreach (Match match in Regex.Matches(input, @"""([^""]+)""|([^ ]+)"))
                    {
                        string arg;
                        if (string.IsNullOrEmpty(match.Groups[1].Value))
                            arg = match.Groups[0].Value;
                        else
                            arg = match.Groups[1].Value;

                        args.Add(arg);
                    }

                    run = commandHelper.HandleCommand(args.ToArray());
                }
            }
            catch (Exception ex)
            {
                _io.WriteString(ex.Message);
            }

            try
            {
                socket.Close();
            }
            catch { }

            _io.WriteString("Connection with server closed");
        }
    }
}