using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ProgSieciowe.Server;
using System.Net;

var address = IPAddress.Loopback;
var port = 1050;
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());


var server = new Server(address, port, "C:\\Users\\Legion\\Desktop\\Temp", loggerFactory);

server.StartAsync().Wait();