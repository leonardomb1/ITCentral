using System.Text.Json;
using System.Text.Json.Serialization;

namespace ITCentral.Models;

public class Message<T> : IDisposable
{
    public short Status { get; private set; }
    public string Information { get; private set; }
    public bool Error { get; private set; }
    public int? EntityCount { get; private set; }
    public List<T>? Content { get; private set; }
    private readonly bool disposed = false;

    public Message(
        short statusId,
        string info,
        bool err,
        List<T> values
    )
    {
        Status = statusId;
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