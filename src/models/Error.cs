using System.Runtime.CompilerServices;

namespace ITCentral.Models;

public sealed class Error : IDisposable
{
    public string ExceptionMessage {get; private set;}
    public string? StackTrace {get; private set;}
    public string FaultedMethod {get; private set;}
    public bool IsPartialSuccess {get; private set;}
    public Dictionary<string, object> UsedArguments {get; private set;}
    private readonly bool disposed = false;

    public Error(
        string msg,
        string? stk,
        bool partialSuccess,
        [CallerMemberName] string? method = null
    ) 
    {
        ExceptionMessage = msg;
        StackTrace = stk;
        IsPartialSuccess = partialSuccess;
        FaultedMethod = method ?? "n/a";
        
        var methodInfo = GetType().GetMethod(method ?? string.Empty);
        UsedArguments = methodInfo?
            .GetParameters()
            .ToDictionary(p => p.Name ?? "Unknown", p => (object)p.Attributes)
            ?? [];
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed && !disposing)
        {
            if (UsedArguments != null)
            {
                foreach (var item in UsedArguments.Values.OfType<IDisposable>())
                {
                    item.Dispose();
                }
                UsedArguments.Clear();
            }
        }
    } 

    ~Error()
    {
        Dispose(false);
    }
}