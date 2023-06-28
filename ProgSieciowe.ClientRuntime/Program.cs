using ProgSieciowe.Client;
using ProgSieciowe.ClientRuntime;
using ProgSieciowe.Core.Enums;
using System.Net;

if(args.Length == 0)
{
    args = new[] { "127.0.0.1", "1050", "tcp" };
}

var address = IPAddress.Parse(args[0]);
var port = int.Parse(args[1]);
var protocol = args[2] == "udp" ? Protocol.Udp : Protocol.Tcp;
var consoleInput = new ConsoleInputOutput();

var client = new Client(protocol, address, port, consoleInput);
client.Start();