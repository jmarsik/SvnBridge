using System;

namespace SvnBridge
{
    public class StartOptions
    {
        private bool displayGui;
        private int port;
        private string tfsUrl;

        private const string usage = "usage: SvnBridge.exe [<port> <tfsUrl>] [/gui|/gui-]"; // TODO: localize

        public StartOptions(string[] commandLineArguments)
        {
            displayGui = true;

            if (commandLineArguments == null)
                throw new ArgumentNullException("commandLineArguments");

            if (commandLineArguments.Length == 0)
                return;

            if (commandLineArguments.Length == 1)
            {
                ReadDisplayGuiFlag(commandLineArguments[0]);
                return;
            }

            if (commandLineArguments.Length == 3)
                ReadDisplayGuiFlag(commandLineArguments[2]);

            if (commandLineArguments.Length > 3)
                throw new StartOptionsException(usage, !HasHideGuiFlag(commandLineArguments));

            ReadPort(commandLineArguments[0]);

            ReadTfsUrl(commandLineArguments[1]);
        }

        public bool DisplayGui
        {
            get { return displayGui; }
        }

        public int Port
        {
            get { return port; }
        }

        public string TfsUrl
        {
            get { return tfsUrl; }
        }

        private static bool HasHideGuiFlag(string[] commandLineArguments)
        {
            foreach (string commandLineArgument in commandLineArguments)
            {
                if (commandLineArgument == "/gui-" || commandLineArgument == "/g-")
                    return true;
            }

            return false;
        }

        private void ReadDisplayGuiFlag(string displayGuiFlagArgument)
        {
            if (displayGuiFlagArgument == "/gui" || displayGuiFlagArgument == "/g")
                displayGui = true;
            else if (displayGuiFlagArgument == "/gui-" || displayGuiFlagArgument == "/g-")
                displayGui = false;
            else
                throw new StartOptionsException(usage, DisplayGui);
        }

        private void ReadPort(string portArgument)
        {  
            if (!int.TryParse(portArgument, out port))
                throw new StartOptionsException("Invalid port: must be a number", DisplayGui); // TODO: localize

            if (port < 1 || port > Constants.MaxPort)
                throw new StartOptionsException("Invalid port: must be between 1 and 65535", DisplayGui); // TODO: localize
        }

        private void ReadTfsUrl(string tfsUrlArgument)
        {
            Uri uri;

            if (!Uri.TryCreate(tfsUrlArgument, UriKind.Absolute, out uri))
                throw new StartOptionsException("Invalid TFS server URL: must be a valid, absolute URL", DisplayGui); // TODO: localize

            tfsUrl = uri.ToString();
        }
    }

    public class StartOptionsException : Exception
    {
        private readonly bool displayGui;
        
        public StartOptionsException(string message, bool displayGui)
            : base(message)
        {
            this.displayGui = displayGui;
        }

        public bool DisplayGui
        {
            get { return displayGui; }
        }
    }
}
