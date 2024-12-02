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

        Action switcher = args.FirstOrDefault() switch {
            string o when o == "-e" => AppCommon.InitializeFromEnv,
            string o when o == "-f" => () => AppCommon.InitializeFromYaml(args.ElementAt(1)),
            string o when o == "-c" => () => AppCommon.InitializeFromArgs([ .. args.Skip(1) ]),
            null => AppCommon.InitializeFromEnv,
            _ => () => throw new Exception("Not Supported")
        };
        
        switcher.Invoke();

        Server server = new();
        server.Run();
    }
}