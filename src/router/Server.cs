using ITCentral.Common;
using ITCentral.Controller;
using WatsonWebserver;
using WatsonWebserver.Core;
using WebMethod = WatsonWebserver.Core.HttpMethod;

namespace ITCentral.Router;

internal class Server
{
    private readonly Webserver service;
    protected internal Server()
    {
        service = new Webserver(
            new WebserverSettings(AppCommon.HostName, AppCommon.PortNumber, AppCommon.Ssl),
            GenericController.NotFound
        );

        // User model routes
        var userController = new UserController();
        service.Routes.PreAuthentication.Static.Add(WebMethod.POST, "/api/users/login", userController.Login, GenericController.ErrorDefaultRoute);
        service.Routes.PostAuthentication.Static.Add(WebMethod.GET, "/api/users", userController.Get, GenericController.ErrorDefaultRoute);
        service.Routes.PostAuthentication.Static.Add(WebMethod.POST, "/api/users", userController.Post, GenericController.ErrorDefaultRoute);
        service.Routes.PostAuthentication.Parameter.Add(WebMethod.GET, "/api/users/id/{userId}", userController.GetById, GenericController.ErrorDefaultRoute);
        service.Routes.PostAuthentication.Parameter.Add(WebMethod.GET, "/api/users/name/{userName}", userController.GetByName, GenericController.ErrorDefaultRoute);
        service.Routes.PostAuthentication.Parameter.Add(WebMethod.PUT, "/api/users/{userId}", userController.Put, GenericController.ErrorDefaultRoute);
        service.Routes.PostAuthentication.Parameter.Add(WebMethod.DELETE, "/api/users/{userId}", userController.Delete, GenericController.ErrorDefaultRoute);        
        
        // Authentication
        service.Routes.AuthenticateRequest = GenericController.Authenticate;
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