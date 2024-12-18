using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class ExtractionService : ServiceBase, IService<Extraction, int>, IDisposable
{
    private readonly bool disposed = false;

    public ExtractionService() : base() { }

    public async Task<Result<List<Extraction>, Error>> Get(Dictionary<string, string?>? filters)
    {
        try
        {
            var select = from e in Repository.Extractions
                         .LoadWith(e => e.Schedule)
                         .LoadWith(e => e.Origin)
                         .LoadWith(e => e.Destination)
                         select e;

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    select = filter.Key.ToLower() switch
                    {
                        "name" => select.Where(e => e.Name == filter.Value),
                        "scheduleId" when int.TryParse(filter.Value, out var schId) => select.Where(e => e.ScheduleId == schId),
                        "originId" when int.TryParse(filter.Value, out var oriId) => select.Where(e => e.OriginId == oriId),
                        "destinationId" when int.TryParse(filter.Value, out var destId) => select.Where(e => e.DestinationId == destId),
                        _ => select
                    };
                }
            }

            return await select.ToListAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<Extraction?, Error>> Get(int id)
    {
        try
        {
            var select = from e in Repository.Extractions
                         .LoadWith(e => e.Schedule)
                         .LoadWith(e => e.Origin)
                         .LoadWith(e => e.Destination)
                         where e.Id == id
                         select e;

            return await select.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Post(Extraction extraction)
    {
        try
        {
            var insert = await Repository.InsertAsync(extraction);
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Put(Extraction extraction, int id)
    {
        try
        {
            extraction.Id = id;

            await Repository.UpdateAsync(extraction);

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Delete(int id)
    {
        try
        {
            await Repository.Extractions
                .Where(e => e.Id == id)
                .DeleteAsync();

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
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
            Repository.Dispose();
        }
    }

    ~ExtractionService()
    {
        Dispose(false);
    }
}