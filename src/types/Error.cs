using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ITCentral.Types;

public sealed class Error : IDisposable
{
    [JsonPropertyName("Message")]
    public string ExceptionMessage {get; set;}

    [JsonIgnore]
    public string? StackTrace {get; set;}
    
    [JsonIgnore]
    public string FaultedMethod {get; set;}
    
    [JsonIgnore]
    public bool IsPartialSuccess {get; set;}
    
    [JsonIgnore]
    public Dictionary<string, object> UsedArguments {get; set;}
    
    [JsonIgnore]
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