using ITCentral.Common;
using ITCentral.Data;

namespace ITCentral.Service;

public abstract class ServiceBase<M>
{
    protected readonly IDBCall Repository;
    public ServiceBase()
    {
        Repository = AppCommon.DbConfig();
        var lookup = Repository.SyncLookup<M>()
            .GetAwaiter()
            .GetResult();
        if(!lookup.IsSuccessful) {
            throw new Exception(
                $"Service Initialization Error: {lookup.Error.FaultedMethod} - {lookup.Error.ExceptionMessage}" 
            );
        }
        if(!lookup.Value) {
            Repository.CreateTable<M>();
        }
    }
}
