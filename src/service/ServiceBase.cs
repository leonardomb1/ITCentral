using ITCentral.Common;

namespace ITCentral.Service;

public abstract class ServiceBase
{
    public ServiceBase(Type type) 
    {
        var callerInstance = AppCommon.GenerateCallerInstance();
        Type instanceType = callerInstance.GetType();
        repositoryType = type.MakeGenericType(instanceType);

        repositoryInstance = Activator.CreateInstance(repositoryType!, callerInstance);
    }
    protected object? repositoryInstance;
    protected Type? repositoryType;
}