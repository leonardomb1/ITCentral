using ITCentral.Common;
using ITCentral.Router;

namespace ITCentral;

public class Program
{
    public static void Main(string[] args)
    {

        if (args.Contains("--help") || args.Contains("-h")) {
            AppCommon.ShowHelp();
            return;
        }

        if (args.Contains("--version") || args.Contains("-v")) {
            AppCommon.ShowVersion();
            return;
        }

        bool isCmd = 
                    args.Contains("--environment") || 
                    args.Contains("-e") || 
                    args.Length > 0;

        Run(isCmd, args);
    }

    private static void Run(bool isCmd, string[] args)
    {
        if(!isCmd) {
            AppCommon.Initialize();
        } else {
            var values = args.Skip(1);

            if(!int.TryParse(values.ElementAt(0), null, out int port)) throw new Exception("Value must be an integer.");
            AppCommon.ConnectionString = values.ElementAt(1);
            if(!bool.TryParse(values.ElementAt(2), out bool ssl)) throw new Exception("Value must be a boolean.");
            AppCommon.PortNumber = port;
            AppCommon.Ssl = ssl;
            AppCommon.HostName = values.ElementAt(3);
        }

        Server server = new();
        server.Run();
    }
}