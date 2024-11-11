using ITCentral.Data;

namespace ITCentral.Service;

public abstract class ServiceBase<T, M> where T : IDBCall
{
    protected readonly T dbCaller;
    public ServiceBase(T db)
    {
        dbCaller = db;
        var lookup = dbCaller.SyncLookup<M>()
            .GetAwaiter()
            .GetResult();
        if(!lookup.IsSuccessful) {
            throw new Exception(
                $"Service Initialization Error: {lookup.Error.FaultedMethod} - {lookup.Error.ExceptionMessage}" 
            );
        }
        if(!lookup.Value) {
            dbCaller.CreateTable<M>();
        }
    }
}