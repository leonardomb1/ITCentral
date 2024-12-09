using System.Text.Json;
using ITCentral.Types;

namespace ITCentral.Common;

public static class Converter
{
    public static Result<T, Error> TryDeserializeJson<T>(string data)
    {
        try {
            return JsonSerializer.Deserialize<T>(data)!;
        } catch (Exception ex) {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
}