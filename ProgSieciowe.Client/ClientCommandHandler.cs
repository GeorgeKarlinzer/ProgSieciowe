﻿using ProgSieciowe.Core;
using ProgSieciowe.Core.Enums;

namespace ProgSieciowe.Client
{
    internal class ClientCommandHandler
    {
        private readonly ICommunicator _communicator;
        private readonly IInputOutput _io;

        public ClientCommandHandler(ICommunicator communicator, IInputOutput io)
        {
            _communicator = communicator;
            _io = io;
        }

        private static CommandType ParseCommand(string command)
        {
            if (command == nameof(CommandType.help))
                return CommandType.help;

            if (command == nameof(CommandType.ls))
                return CommandType.ls;

            if (command == nameof(CommandType.delete))
                return CommandType.delete;

            if (command == nameof(CommandType.rename))
                return CommandType.rename;

            if (command == nameof(CommandType.download))
                return CommandType.download;

            if (command == nameof(CommandType.upload))
                return CommandType.upload;

            if (command == nameof(CommandType.exit))
                return CommandType.exit;

            return (CommandType)(-1);
        }

        public bool HandleCommand(string[] args)
        {
            var commandType = ParseCommand(args[0]);

            return commandType switch
            {
                CommandType.help => HandleHelp(),
                CommandType.ls => HandleLs(),
                CommandType.delete => HandleDelete(args),
                CommandType.rename => HandleRename(args),
                CommandType.download => HandleDownload(args),
                CommandType.upload => HandleUpload(args),
                CommandType.exit => HandleExit(),
                _ => HandleUnknown()
            };
        }

        private bool HandleHelp()
        {
            _communicator.Send($"{(int)CommandType.help}");
            var msg = _communicator.ReceiveString();
            _io.WriteString(msg);
            return true;
        }

        private bool HandleLs()
        {
            _communicator.Send($"{(int)CommandType.ls}");
            var msg = _communicator.ReceiveString();
            _io.WriteString(msg);
            return true;
        }

        private bool HandleDelete(string[] args)
        {
            if(args.Length == 1)
            {
                _io.WriteString("Missing argument [file]");
            }

            var file = args[1];

            _communicator.Send($"{(int)CommandType.delete}");
            _communicator.Send(file);

            var res = _communicator.ReceiveString();
            _io.WriteString(res);

            return true;
        }

        private bool HandleRename(string[] args)
        {
            if (args.Length == 1)
            {
                _io.WriteString("Missing arguments [file] [new_name]");
                return true;
            }

            if(args.Length == 2)
            {
                _io.WriteString("Missing argument [new_name]");
                return true;
            }

            var file = args[1];
            var newName = args[2];

            _communicator.Send($"{(int)CommandType.rename}");
            _communicator.Send($"{file}|{newName}");

            var res = _communicator.ReceiveString();
            _io.WriteString(res);

            return true;
        }

        private bool HandleDownload(string[] args)
        {
            if (args.Length == 1)
            {
                _io.WriteString("Missing argument [file]");
                return true;
            }

            var file = args[1];
            _communicator.Send($"{(int)CommandType.download}");
            _communicator.Send(file);

            var directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, file);

            var status = _communicator.ReceiveString();
            if(status != "1")
            {
                var errorMsg = _communicator.ReceiveString();
                _io.WriteString(errorMsg);
                return true;
            }

            _communicator.ReceiveFile(path);
            _io.WriteString("\tFile was download successfully");

            return true;
        }

        private bool HandleUpload(string[] args)
        {
            if (args.Length == 1)
            {
                _io.WriteString("Missing argument [path]");
            }

            var path = args[1];

            if (!File.Exists(path))
            {
                _communicator.Send("0");
                return true;
            }

            var fileName = Path.GetFileName(path);
            _communicator.Send($"{(int)CommandType.upload}");
            _communicator.Send(fileName);

            var status = _communicator.ReceiveString();
            if(status != "1")
            {
                var errorMsg = _communicator.ReceiveString();
                _io.WriteString(errorMsg);
                return true;
            }

            _communicator.SendFile(path);

            var msg = _communicator.ReceiveString();
            _io.WriteString(msg);

            return true;
        }

        private bool HandleExit()
        {
            _communicator.Send($"{(int)CommandType.exit}");
            _io.WriteString("Closing connection...");
            return false;
        }

        private bool HandleUnknown()
        {
            _io.WriteString("\tUnknown command");
            return true;
        }
    }
}
