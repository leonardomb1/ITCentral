using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;

namespace ITCentral.App.Exchange;

public class DBExchange
{
    public async Task<Result<int, Error> DataTransfer()
    {
        using ExtractionService service = new();

        var result = service.Get();
        if (!result.IsSuccessful) {
            return new Error("No table from which to extract.", null, false);
        }

        List<Extraction> extractions = result.Value;

        
    }
}