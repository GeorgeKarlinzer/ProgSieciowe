using Microsoft.Extensions.Logging;
using ProgSieciowe.Server;
using System.Diagnostics;
using System.Net;

if (args.Length == 0)
{
    args = new[] { "127.0.0.1", "1050", ".\\" };
}

ServerLauncher server = null;
IPAddress address;
int port;
string directory;
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
try
{
    address = IPAddress.Parse(args[0]);
    port = int.Parse(args[1]);
    directory = args[2];
    server = new ServerLauncher(address, port, directory, loggerFactory);
}
catch
{
    Console.WriteLine("Wrong arguments, try:");
    Console.WriteLine($"\tProgram.exe <ip_address> <port> <working_direcory>");
    return;
}
server.Launch();
Console.WriteLine("Press [ENTER] to close the application");
Console.ReadLine();