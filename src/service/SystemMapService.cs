using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Service;

public class SystemMapService : ServiceBase<SystemMap>, IService<SystemMap, int>
{
    public SystemMapService() : base() { }
    public async Task<Result<List<SystemMap>, Error>> Get()
    {
        var select = await Repository.ReadFromDb<SystemMap>();
        if(!select.IsSuccessful) {
            return select.Error;
        }

        return select.Value!;
    }
    public async Task<Result<SystemMap?, Error>> GetById(int id)
    {
        var selectById = await Repository.ReadFromDb<SystemMap, int>("id", id);
        if(!selectById.IsSuccessful) {
            return selectById.Error;
        }

        return selectById.Value.FirstOrDefault();
    }
    public async Task<Result<bool, Error>> Post(SystemMap system)
    {
        var insert = await Repository.Insert(system);
        if(!insert.IsSuccessful) {
            return insert.Error;
        }

        return AppCommon.Success;
    }
    public async Task<Result<SystemMap?, Error>> Put(SystemMap system, int id)
    {
        var select = await Repository.CheckRecord<SystemMap, int>("id", id);
        if(!select.IsSuccessful) {
            return select.Error;
        }

        system.Id = id;
        var update = await Repository.Update("id", system, id);
        if(!update.IsSuccessful) {
            return update.Error;
        }

        return update.Value;
    }
    public async Task<Result<bool, Error>> Delete(int id)
    {
        var delete = await Repository.DeleteFromDb<SystemMap, int>("id", id);
        if(!delete.IsSuccessful) {
            return delete.Error;
        }

        return delete.Value;
    }
}