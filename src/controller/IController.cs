namespace ITCentral.Controller;

internal interface IController<T>
{
    public Task Get(T ctx);
    public Task GetById(T ctx);
    public Task Post(T ctx);
    public Task Put(T ctx);
    public Task Delete(T ctx);
}