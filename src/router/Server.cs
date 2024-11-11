using ITCentral.Common;
using ITCentral.Models;
using WatsonWebserver;
using WatsonWebserver.Core;
using WebMethod = WatsonWebserver.Core.HttpMethod;

namespace ITCentral.Router;

internal class Server : Routes
{
    private readonly Webserver service;
    protected internal Server()
    {
        service = new Webserver(
            new WebserverSettings(AppCommon.HostName, AppCommon.PortNumber, AppCommon.Ssl),
            NotFound
        );

        service
            .Routes
            .PreAuthentication
            .Static
            .Add(WebMethod.GET, "/api", Test);
            
        service
            .Routes
            .PreAuthentication
            .Static
            .Add(WebMethod.GET, "/api/cats", (ctx) => Get(ctx, nameof(Cat)));

        service
            .Routes
            .PreAuthentication
            .Static
            .Add(WebMethod.POST, "/api/cats", (ctx) => Create(ctx, nameof(Cat)));

        service
            .Routes
            .PreAuthentication
            .Parameter
            .Add(WebMethod.GET, "/api/cats/{catId}", (ctx) => GetById(ctx, nameof(Cat)));
 
        service
            .Routes
            .PreAuthentication
            .Parameter
            .Add(WebMethod.PUT, "/api/cats/{catId}", (ctx) => UpdateById(ctx, nameof(Cat)));

        service
            .Routes
            .PreAuthentication
            .Parameter
            .Add(WebMethod.DELETE, "/api/cats/{catId}", (ctx) => DeleteById(ctx, nameof(Cat)));
    }

    public void Run()
    {
        service.Start();
        Log.Out($"Service is running at port: {AppCommon.PortNumber}");
        Console.ReadLine();
    }

    ~Server()
    {
        service.Dispose();
    }
}