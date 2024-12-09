using ITCentral.Common;
namespace ITCentral;

public class Program
{   
    public static void Main(string[] args)
    {
        Action switcher = args.FirstOrDefault() switch {
            string o when o == "-h" || o == "--help" => AppCommon.ShowHelp,
            string o when o == "-v" || o == "--version" => AppCommon.ShowVersion,
            string o when o == "-e" || o == "--environment" => AppCommon.InitializeFromEnv,
            string o when o == "-f" || o == "--file" => () => AppCommon.InitializeFromYaml(args.ElementAt(1)),
            string o when o == "-c" || o == "--console" => () => AppCommon.InitializeFromArgs([ .. args.Skip(1) ]),
            null => AppCommon.InitializeFromEnv,
            _ => () => {Console.WriteLine("This option is invalid."); AppCommon.ShowHelp();}
        };
        
        switcher.Invoke();
    }
}