using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Service;

internal interface IService<T, ID>
{
    public Task<Result<List<T>, Error>> Get();
    public Task<Result<T, Error>> GetById(ID id);
    public Task<Result<T, Error>> Post(T ctx);
    public Task<Result<T, Error>> Put(T ctx, ID id);
    public Task<Result<T, Error>> Delete(ID id);
}