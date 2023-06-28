using ProgSieciowe.ServerService;
using Serilog;

public class Program
{
    private static void Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var options = configuration.GetSection("Config").Get<Config>();

                services.AddSingleton(options!);
                services.AddHostedService<Worker>();
                services.AddWindowsService(options => options.ServiceName = "TCP/UDP File Manager");
            })
            .UseSerilog((context, configuration) =>
                configuration.WriteTo.File("logs/log.txt"))
            .Build();

        var lf = host.Services.GetService<ILoggerFactory>();
        var l = lf.CreateLogger<Program>();
        l.LogInformation("test");

        host.Run();
    }
}