using System.Runtime.CompilerServices;

namespace ITCentral.Common;

public static class Log
{
    private static string LogPrefix() => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff}]::";
    public static void Out(
        string message,
        [CallerMemberName] string? callerMethod = null
    )
    {
        string log = LogPrefix() + $"[{callerMethod}] > " + message;
        Console.WriteLine(log);
    }
}