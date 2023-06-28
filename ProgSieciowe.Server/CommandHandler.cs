using Microsoft.Extensions.Logging;
using ProgSieciowe.Core;
using ProgSieciowe.Core.Enums;
using System.Runtime.CompilerServices;

namespace ProgSieciowe.Server
{
    internal class ServerCommandHandler
    {
        private readonly ICommunicator _communicator;
        private readonly ILogger _logger;
        private readonly string _directory;

        public ServerCommandHandler(ICommunicator communicator, ILoggerFactory loggerFactory, string directory)
        {
            _communicator = communicator;
            _logger = loggerFactory.CreateLogger<ServerCommandHandler>();
            _directory = directory;
        }

        private void HandleHandlableError(Exception ex, [CallerMemberName] string callerName = "")
        {
            var msg = $"Error during {callerName}";
            _communicator.Send("\t" + msg);
            _logger.LogError(ex, msg);
        }

        private bool CheckFileExisting(string path)
        {
            if (!File.Exists(path))
            {
                _communicator.Send("\tFile does not exist");
                return false;
            }

            return true;
        }

        private string GetFileName(string file) =>
            Path.Combine(_directory, file);

        public bool HandleCommand(CommandType commandType) =>
            commandType switch
            {
                CommandType.help => HandleHelp(),
                CommandType.ls => HandleLs(),
                CommandType.delete => HandleDelete(),
                CommandType.rename => HandleRename(),
                CommandType.download => HandleDownload(),
                CommandType.upload => HandleUpload(),
                CommandType.exit => HandleExit(),
                _ => HandleUnknown()
            };

        private bool HandleHelp()
        {
            var msg = """
                        help
                        ls
                        delete [file]
                        rename [file] [new_name] 
                        download [file]
                        upload [path]
                        exit
                """;

            _communicator.Send(msg);
            return true;
        }

        private bool HandleLs()
        {
            var files = Directory.GetFiles(_directory);
            var msg = string.Join('\n', files.Select(x => $"\t{Path.GetFileName(x)}"));
            if (string.IsNullOrEmpty(msg))
                msg = "\tDirectory is empty";

            _communicator.Send(msg);

            return true;
        }

        private bool HandleDelete()
        {
            var file = _communicator.ReceiveString();
            var path = GetFileName(file);

            if (!CheckFileExisting(path))
                return true;

            try
            {
                File.Delete(GetFileName(file));
                _communicator.Send("\tSuccessfully deleted file");
            }
            catch (Exception ex)
            {
                HandleHandlableError(ex);
            }

            return true;
        }

        private bool HandleRename()
        {
            var input = _communicator.ReceiveString();
            var oldName = input.Split('|').First();
            var newName = input.Split('|').Last();

            if (!File.Exists(GetFileName(oldName)))
            {
                _communicator.Send("\tInvalid file name");
                return true;
            }

            try
            {
                File.Move(GetFileName(oldName), GetFileName(newName));
                _communicator.Send("\tSuccessfully renamed file");
            }
            catch (Exception ex)
            {
                HandleHandlableError(ex);
            }

            return true;
        }

        private bool HandleDownload()
        {
            var file = _communicator.ReceiveString();
            var path = GetFileName(file);
            _logger.LogInformation("File to download: {file}", file);

            if (!File.Exists(path))
            {
                _communicator.Send("0");
                _communicator.Send("\tFile does not exists");
                return true;
            }

            _communicator.Send("1");
            _communicator.SendFile(path);

            return true;
        }

        private bool HandleUpload()
        {
            var file = _communicator.ReceiveString();

            _communicator.Send("1");
            var path = GetFileName(file);

            _communicator.ReceiveFile(path);

            _communicator.Send("\tFile was uploded successfully");
            return true;
        }

        private bool HandleExit()
        {
            return false;
        }

        private bool HandleUnknown()
        {
            _communicator.Send("\tUnknown command");
            return true;
        }
    }
}
