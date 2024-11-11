using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Data;

public interface IDBCall
{
    public Task<Result<bool, Error>> CreateTable<T>();
    public Task<Result<bool, Error>> SyncLookup<T>();
    public Task<Result<T, Error>> Insert<T>(T entity);
    public Task<Result<List<T?>, Error>> ReadFromDb<T>() where T : class;
    public Task<Result<T?, Error>> ReadFromDb<T, ID>(ID id) where T : class;
    public Task<Result<T?, Error>> Update<T, ID>(T entity, ID id);
    public Task<Result<bool, Error>> DeleteFromDb<T, ID>(ID id);
}