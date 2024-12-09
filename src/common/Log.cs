using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ITCentral.Common;

public static class Log
{
    private static readonly ConcurrentQueue<string> logQueue = new();
    
    private static readonly string hostname = Environment.MachineName;
    
    private static string LogPrefix() => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff}]::";

    static Log()
    {
        _ = new Timer(DumpLogsToFile, null, AppCommon.LogDumpTime, AppCommon.LogDumpTime);
    }

    public static void Out(
        string message,
        string? logType = null,
        bool dump = true,
        [CallerMemberName] string? callerMethod = null
    ) {
        string type = logType ?? AppCommon.MessageInfo;
        string log = LogPrefix() + $"[{callerMethod}]::[{type}] > {message}";
        Console.WriteLine(log);
        if(dump && AppCommon.Logging) logQueue.Enqueue(log);
    }

    private static void DumpLogsToFile(object? state)
    {
        Out("Log dump routine started.", dump: false);
        if (logQueue.IsEmpty) return;

        using var writer = new StreamWriter(AppCommon.LogFilePath, true);
        try
        {
            while (logQueue.TryDequeue(out var logEntry))
            {
                writer.WriteLine($"[{hostname}] - {logEntry}");
            }
            writer.Flush();
        }
        catch (Exception ex)
        {
            Out($"Error while executing log dump routing: {ex.Message}", AppCommon.MessageError);
        }
    }
}