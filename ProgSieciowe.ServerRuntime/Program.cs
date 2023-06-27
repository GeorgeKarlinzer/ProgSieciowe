using Microsoft.Extensions.Logging;
using ProgSieciowe.Server;
using System.Net;

var address = IPAddress.Parse(args[0]);
var port = int.Parse(args[1]);
var directory = args[2];
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

var server = new Server(address, port, directory, loggerFactory);

server.Start();