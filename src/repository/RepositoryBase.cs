using ITCentral.Data;

namespace ITCentral.Repository;

public abstract class RepositoryBase<T, M> where T : IDBCall
{
    protected readonly T dbCaller;
    public RepositoryBase(T db)
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