using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ITCentral.Models;
using ITCentral.Service;

namespace ITCentral.Common;

public static class Log
{
    private static readonly ConcurrentQueue<Record> logs = [];

    private static readonly string hostname = Environment.MachineName;

    private static string LogPrefix(DateTime time) => $"[{time:yyyy-MM-dd HH:mm:ss:fff}]::";

    private static readonly Timer logDumpTimer = new(async _ =>
        {
            await DumpLogsToFile(null);
        }
        , null, AppCommon.LogDumpTime, AppCommon.LogDumpTime
    );

    static Log()
    {
        logDumpTimer.Change(AppCommon.LogDumpTime, AppCommon.LogDumpTime);
    }

    public static void Out(
        string message,
        string? logType = null,
        bool dump = true,
        [CallerMemberName] string? callerMethod = null
    )
    {
        DateTime executionTime = DateTime.Now;
        string type = logType ?? AppCommon.MessageInfo;
        string log = LogPrefix(executionTime) + $"[{callerMethod}]::[{type}] > {message}";

        Console.WriteLine(log);

        if (dump && AppCommon.Logging)
        {
            Record record = new(hostname, executionTime, type, callerMethod ?? "", message);
            logs.Enqueue(record);
        }
    }

    private static async Task DumpLogsToFile(object? state)
    {
        if (logs.IsEmpty) return;
        Out("Log dump routine started.", dump: false);

        using var recordService = new RecordService();

        try
        {
            var recordsToDump = new List<Record>();
            while (logs.TryDequeue(out var record))
            {
                recordsToDump.Add(record);
            }
            await recordService.Post(recordsToDump);
        }
        catch (Exception ex)
        {
            Out($"Error while executing log dump routing: {ex.Message}", AppCommon.MessageError, dump: false);
        }
    }
}