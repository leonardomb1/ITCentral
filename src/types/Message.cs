using System.Text.Json;
using System.Text.Json.Serialization;

namespace ITCentral.Types;

public sealed class Message<T> : IDisposable
{
    [JsonRequired]
    public short StatusCode { get; set; }
    
    [JsonRequired]
    public string Information { get; set; }
    
    [JsonRequired]
    public bool Error { get; set; }
    
    public int? EntityCount { get; set; }
    
    public List<T>? Content { get; set; }
    
    [JsonIgnore]
    private readonly bool disposed = false;

    public Message(
        short statusId,
        string info,
        bool err,
        List<T>? values = null
    )
    {
        StatusCode = statusId;
        Information = info;
        Error = err;
        Content = values;
        EntityCount = values?.Count;
    }

    private JsonSerializerOptions? options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string AsJsonString()
    {
        return JsonSerializer.Serialize(
            this,
            options
        );
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                options = null;
            }
        }
    }

    ~Message()
    {
        Dispose(false);
    }
}