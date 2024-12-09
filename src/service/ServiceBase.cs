using ITCentral.Data;

namespace ITCentral.Service;

public abstract class ServiceBase
{
    protected readonly CallBase Repository;
    
    public ServiceBase()
    {
        Repository = new CallBase();
    }
}
