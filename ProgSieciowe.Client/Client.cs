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
            try
            {
                ICommunicator communicator;
                var serverEndPoint = new IPEndPoint(_address, _port);
                if (_protocol is Protocol.Tcp)
                {
                    var tcpClient = new TcpClient();
                    tcpClient.Connect(serverEndPoint);
                    tcpClient.ReceiveTimeout = Constants.DefaultTimeOut;
                    tcpClient.SendTimeout = Constants.DefaultTimeOut;

                    communicator = new TcpCommunicator(tcpClient);
                    _io.WriteString("Connection established");
                }
                else
                {
                    var client = new UdpClient();
                    client.Client.ReceiveTimeout = Constants.DefaultTimeOut;
                    client.Client.SendTimeout = Constants.DefaultTimeOut;
                    communicator = new UdpCommunicator(client, serverEndPoint);
                    communicator.Send("start");
                }

                StartCient(communicator);
            }
            catch(Exception ex)
            {
                _io.WriteString(ex.Message);
            }
        }

        private void StartCient(ICommunicator communicator)
        {
            var commandHelper = new ClientCommandHandler(communicator, _io);

            var run = true;
            try
            {
                while (run)
                {
                    _io.WriteString("Enter command:");
                    var input = _io.GetString();
                    var args = new List<string>();

                    foreach (Match match in Regex.Matches(input, @"""([^""]+)""|([^ ]+)").Cast<Match>())
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

            _io.WriteString("Connection with server closed");
        }
    }
}