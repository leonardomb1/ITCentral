using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Service;

public class ScheduleService : ServiceBase<Schedule>, IService<Schedule, int>
{
    public ScheduleService() : base() { }
    public async Task<Result<List<Schedule>, Error>> Get()
    {
        var select = await Repository.ReadFromDb<Schedule>();
        if(!select.IsSuccessful) {
            return select.Error;
        }

        return select.Value!;
    }
    public async Task<Result<Schedule?, Error>> GetById(int id)
    {
        var selectById = await Repository.ReadFromDb<Schedule, int>("id", id);
        if(!selectById.IsSuccessful) {
            return selectById.Error;
        }

        return selectById.Value.FirstOrDefault();
    }
    public async Task<Result<bool, Error>> Post(Schedule system)
    {
        var insert = await Repository.Insert(system);
        if(!insert.IsSuccessful) {
            return insert.Error;
        }

        return AppCommon.Success;
    }
    public async Task<Result<Schedule?, Error>> Put(Schedule system, int id)
    {
        var select = await Repository.CheckRecord<Schedule, int>("id", id);
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
        var delete = await Repository.DeleteFromDb<Schedule, int>("id", id);
        if(!delete.IsSuccessful) {
            return delete.Error;
        }

        return delete.Value;
    }
}