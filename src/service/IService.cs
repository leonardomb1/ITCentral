using ITCentral.Types;

namespace ITCentral.Service;

internal interface IService<T, ID>
{
    public Result<List<T>, Error> Get();
    
    public Result<T?, Error> Get(ID id);
    
    public Result<bool, Error> Post(T ctx);
    
    public Result<bool, Error> Put(T ctx, ID id);
    
    public Result<bool, Error> Delete(ID id);
}