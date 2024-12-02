using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Data;

public interface IDBCall
{
    public Task<Result<bool, Error>> CreateTable<T>();
    public Task<Result<bool, Error>> SyncLookup<T>();
    public Task<Result<T, Error>> Insert<T>(T entity);
    public Task<Result<List<T?>, Error>> ReadFromDb<T>() where T : class;
    public Task<Result<List<T?>, Error>> ReadFromDb<T, V>(string id, V val) where T : class;
    public Task<Result<T?, Error>> Update<T, V>(string id, T entity, V val);
    public Task<Result<bool, Error>> DeleteFromDb<T, V>(string id, V val);
    public Task<Result<bool, Error>> CheckRecord<T, V>(string id, V val);
}