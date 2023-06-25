using ProgSieciowe.Client;
using ProgSieciowe.ClientRuntime;
using ProgSieciowe.Core.Enums;
using System.Net;

var protocol = Protocol.Tcp;
var address = IPAddress.Loopback;
var port = 1050;
var consoleInput = new ConsoleInputOutput();

var client = new Client(protocol, address, port, consoleInput);
client.Start();