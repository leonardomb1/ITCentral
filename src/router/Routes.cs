using ITCentral.Controller;
using WatsonWebserver.Core;

namespace ITCentral.Router;

internal abstract class Routes : GenericController
{
    private readonly Dictionary<string, IController<HttpContextBase>> controllers;

    protected Routes()
    {
        controllers = new Dictionary<string, IController<HttpContextBase>>
        {
            { "Cat", new CatController() },
        };
    }

    private IController<HttpContextBase> GetController(string type)
    {
        if (controllers.TryGetValue(type, out var controller))
        {
            return controller;
        }
        throw new ArgumentException("Unknown controller type", nameof(type));
    }

    protected Task Create(HttpContextBase ctx, string type) => GetController(type).Post(ctx);
    protected Task Get(HttpContextBase ctx, string type) => GetController(type).Get(ctx);
    protected Task GetById(HttpContextBase ctx, string type) => GetController(type).GetById(ctx);
    protected Task UpdateById(HttpContextBase ctx, string type) => GetController(type).Put(ctx);
    protected Task DeleteById(HttpContextBase ctx, string type) => GetController(type).Delete(ctx);
}