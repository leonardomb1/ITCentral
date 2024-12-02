using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Service;

public class ExtractionService : ServiceBase<Extraction>, IService<Extraction, int>
{
    public ExtractionService() : base() { }
    public async Task<Result<List<Extraction>, Error>> Get()
    {
        var select = await Repository.ReadFromDb<Extraction>();
        if(!select.IsSuccessful) {
            return select.Error;
        }

        return select.Value!;
    }
    public async Task<Result<Extraction?, Error>> GetById(int id)
    {
        var selectById = await Repository.ReadFromDb<Extraction, int>("id", id);
        if(!selectById.IsSuccessful) {
            return selectById.Error;
        }

        return selectById.Value.FirstOrDefault();
    }
    public async Task<Result<bool, Error>> Post(Extraction system)
    {
        var insert = await Repository.Insert(system);
        if(!insert.IsSuccessful) {
            return insert.Error;
        }

        return AppCommon.Success;
    }
    public async Task<Result<Extraction?, Error>> Put(Extraction system, int id)
    {
        var select = await Repository.CheckRecord<Extraction, int>("id", id);
        if(!select.IsSuccessful) {
            return select.Error;
        }

        var update = await Repository.Update("id", system, id);
        if(!update.IsSuccessful) {
            return update.Error;
        }

        return update.Value;
    }
    public async Task<Result<bool, Error>> Delete(int id)
    {
        var delete = await Repository.DeleteFromDb<Extraction, int>("id", id);
        if(!delete.IsSuccessful) {
            return delete.Error;
        }

        return delete.Value;
    }
}