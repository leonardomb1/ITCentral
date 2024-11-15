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
            AppCommon.InitializeFromEnv();
        } else {
            AppCommon.InitializeFromArgs(args);
        }

        Server server = new();
        server.Run();
    }
}