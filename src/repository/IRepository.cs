using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Repository;

internal interface IRepository<T, ID>
{
    public Task<Result<T?, Error>> Save(T data);
    public Task<Result<T?, Error>> Save(T data, ID id);
    public Task<Result<List<T?>, Error>> Read();
    public Task<Result<T?, Error>> Read(ID id);
    public Task<Result<bool, Error>> Delete(ID id);
}