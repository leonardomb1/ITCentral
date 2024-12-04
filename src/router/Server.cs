using ITCentral.Common;
using ITCentral.Controller;
using WatsonWebserver;
using WatsonWebserver.Core;
using WebMethod = WatsonWebserver.Core.HttpMethod;
using static ITCentral.Controller.GenericController;

namespace ITCentral.Router;

internal class Server
{
    private readonly ManualResetEvent shutdownEvent = new(false);
    private readonly Webserver service;
    protected internal Server()
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
        string users = root+"/users";
        preAuth.Static.Add(WebMethod.POST, users+"/login", userController.Login, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.GET, users, userController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.POST, users, userController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, users+"/id/{userId}", userController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, users+"/name/{userName}", userController.GetByName, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.PUT, users+"/id/{userId}", userController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.DELETE, users+"/id/{userId}", userController.Delete, ErrorDefaultRoute);        
        
        // SystemMap model routes
        var systemMapController = new SystemMapController();
        string system = root+"/systems";
        postAuth.Static.Add(WebMethod.GET, system, systemMapController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.POST, system, systemMapController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, system+"/id/{systemId}", systemMapController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.PUT, system+"/id/{systemId}", systemMapController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.DELETE, system+"/id/{systemId}", systemMapController.Delete, ErrorDefaultRoute);        

        // Schedule model routes
        var scheduleController = new ScheduleController();
        string schedule = root+"/schedules";
        postAuth.Static.Add(WebMethod.GET, schedule, scheduleController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.POST, schedule, scheduleController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, schedule+"/id/{scheduleId}", scheduleController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.PUT, schedule+"/id/{scheduleId}", scheduleController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.DELETE, schedule+"/id/{scheduleId}", scheduleController.Delete, ErrorDefaultRoute);        

        // Schedule model routes
        var extractionController = new ExtractionController();
        string extraction = root+"/extractions";
        postAuth.Static.Add(WebMethod.GET, extraction, extractionController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(WebMethod.POST, extraction, extractionController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.GET, extraction+"/id/{extractionId}", extractionController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.PUT, extraction+"/id/{extractionId}", extractionController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(WebMethod.DELETE, extraction+"/id/{extractionId}", extractionController.Delete, ErrorDefaultRoute);

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