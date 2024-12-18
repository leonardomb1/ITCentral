using ITCentral.Types;

namespace ITCentral.Service;

internal interface IService<T, ID>
{
    public Task<Result<List<T>, Error>> Get(Dictionary<string, string?>? filters = null);

    public Task<Result<T?, Error>> Get(ID id);

    public Task<Result<bool, Error>> Post(T ctx);

    public Task<Result<bool, Error>> Put(T ctx, ID id);

    public Task<Result<bool, Error>> Delete(ID id);
}