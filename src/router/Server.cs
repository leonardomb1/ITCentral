using ITCentral.Common;
using ITCentral.Controller;
using WatsonWebserver;
using WatsonWebserver.Core;
using WebMethod = WatsonWebserver.Core.HttpMethod;
using static ITCentral.Controller.GenericController;

namespace ITCentral.Router;

public sealed class Server
{
    private readonly ManualResetEvent shutdownEvent = new(false);

    private readonly Webserver service;

    public Server()
    {
        service = new Webserver(
            new WebserverSettings(AppCommon.HostName, AppCommon.PortNumber, AppCommon.Ssl),
            NotFound
        );

        // Options Request
        service.Routes.Preflight = Options;

        // Default routing managers
        var postAuth = service.Routes.PostAuthentication;
        var preAuth = service.Routes.PreAuthentication;

        string root = "/api";

        // User model routes
        var userController = new UserController();
        string users = root + "/users";
        preAuth.Static.Add(WebMethod.POST, root + "/login", userController.Login, ErrorDefaultRoute);
        preAuth.Static.Add(WebMethod.POST, root + "/ssologin", userController.LoginWithLdap, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.GET, users, userController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.POST, users, userController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, users + "/{userId}", userController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, users + "?name={userName}", userController.GetByName, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.PUT, users + "/{userId}", userController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.DELETE, users + "/{userId}", userController.Delete, ErrorDefaultRoute);

        // Origin model routes
        var originController = new OriginController();
        string origin = root + "/origins";
        postAuth.Static.Add(WebMethod.GET, origin, originController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.POST, origin, originController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, origin + "/{originId}", originController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.PUT, origin + "/{originId}", originController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.DELETE, origin + "/{originId}", originController.Delete, ErrorDefaultRoute);

        // Schedule model routes
        var scheduleController = new ScheduleController();
        string schedule = root + "/schedules";
        postAuth.Static.Add(WebMethod.GET, schedule, scheduleController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.POST, schedule, scheduleController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, schedule + "/{scheduleId}", scheduleController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.PUT, schedule + "/{scheduleId}", scheduleController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.DELETE, schedule + "/{scheduleId}", scheduleController.Delete, ErrorDefaultRoute);

        // Schedule model routes
        var destinationController = new DestinationController();
        string destination = root + "/destinations";
        postAuth.Static.Add(WebMethod.GET, destination, destinationController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.POST, destination, destinationController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, destination + "/{destinationId}", destinationController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.PUT, destination + "/{destinationId}", destinationController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.DELETE, destination + "/{destinationId}", destinationController.Delete, ErrorDefaultRoute);

        // Extraction model routes
        var extractionController = new ExtractionController();
        string extraction = root + "/extractions";
        postAuth.Static.Add(WebMethod.GET, extraction, extractionController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.POST, extraction, extractionController.Post, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.GET, extraction + "/execute/{extractionId}", extractionController.ExecuteExtractionByScheduleId, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, extraction + "/execute", extractionController.ExecuteExtractionByNameOrDestination, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, extraction + "/{extractionId}", extractionController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.PUT, extraction + "/{extractionId}", extractionController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.DELETE, extraction + "/{extractionId}", extractionController.Delete, ErrorDefaultRoute);

        // Authentication
        service.Routes.AuthenticateRequest = Authenticate;
    }

    public void Run()
    {
        service.Start();
        Log.Out($"Service is running at port: {AppCommon.PortNumber}");
        AppDomain.CurrentDomain.ProcessExit += (s, e) => shutdownEvent.Set();
        shutdownEvent.WaitOne();
    }

    ~Server()
    {
        service.Dispose();
    }
}