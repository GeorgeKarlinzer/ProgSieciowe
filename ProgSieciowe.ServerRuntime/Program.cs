using Microsoft.Extensions.Logging;
using ProgSieciowe.Server;
using System.Net;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            args = new[] { "127.0.0.1", "1050", ".\\" };
        }

        ServerLauncher launcher = null;
        IPAddress address;
        int port;
        string directory;
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        try
        {
            address = IPAddress.Parse(args[0]);
            port = int.Parse(args[1]);
            directory = args[2];
            launcher = new ServerLauncher(address, port, directory, loggerFactory);
        }
        catch
        {
            Console.WriteLine("Wrong arguments, try:");
            Console.WriteLine($"\tProgram.exe <ip_address> <port> <working_direcory>");
            return;
        }
        launcher.Launch();
        Console.WriteLine("Press [ENTER] to close the application");
        Console.ReadLine();
    }
}