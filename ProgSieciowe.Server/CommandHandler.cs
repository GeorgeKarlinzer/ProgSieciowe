using Microsoft.Extensions.Logging;
using ProgSieciowe.Core;
using ProgSieciowe.Core.Enums;
using ProgSieciowe.Server.Extensions;

namespace ProgSieciowe.Server
{
    internal class CommandHandler
    {
        private readonly ICommunicator _communicator;
        private readonly ILogger _logger;
        private readonly string _directory;

        public CommandHandler(ICommunicator communicator, ILoggerFactory loggerFactory, string directory)
        {
            _communicator = communicator;
            _logger = loggerFactory.CreateLogger<CommandHandler>();
            _directory = directory;
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

            if (file.IsValidFileName())
            {
                File.Delete(GetFileName(file));
                _communicator.Send("\tSuccessfully deleted file");
            }
            else
            {
                _communicator.Send("\tInvalid file name");
            }

            return true;
        }

        private bool HandleRename()
        {
            var input = _communicator.ReceiveString();
            var oldName = input.Split('|').First();
            var newName = input.Split('|').Last();

            if (oldName.IsValidFileName() && newName.IsValidFileName())
            {
                File.Move(GetFileName(oldName), GetFileName(newName));
                _communicator.Send("\tSuccessfully renamed file");
            }
            else
            {
                _communicator.Send("\tInvalid file name");
            }
            return true;
        }

        private bool HandleDownload()
        {
            var file = _communicator.ReceiveString();
            var path = GetFileName(file);
            _logger.LogInformation("File to download: {file}", file);
            if (!file.IsValidFileName())
            {
                _communicator.Send("0");
                _communicator.Send("\tInvalid file name");
                return true;
            }

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
            if (!file.IsValidFileName())
            {
                _communicator.Send("0");
                _communicator.Send("\tInvalid file name");
                return true;
            }

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
