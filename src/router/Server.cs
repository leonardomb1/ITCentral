using ITCentral.Common;
using ITCentral.Controller;
using WatsonWebserver;
using WatsonWebserver.Core;
using static WatsonWebserver.Core.HttpMethod;
using static ITCentral.Controller.GenericController;

namespace ITCentral.Router;

public sealed class Server
{
    private readonly ManualResetEvent shutdownEvent = new(false);

    private readonly Webserver service;

    private readonly string root = "/api";

    public Server()
    {
        service = new Webserver(
            new WebserverSettings(AppCommon.HostName, AppCommon.PortNumber, AppCommon.Ssl),
            NotFound
        );

        // Options PreFlight
        service.Routes.Preflight = Options;

        // CORS Configuration
        service.Settings.AccessControl.Mode = AccessControlMode.DefaultDeny;

        foreach (string ip in AppCommon.AllowedIps.Split("|"))
        {
            service.Settings.AccessControl.PermitList.Add(ip, AppCommon.NetworkIpMask);
        }

        // Default routing managers
        var preAuth = service.Routes.PreAuthentication;
        var postAuth = service.Routes.PostAuthentication;

        // Login routes
        preAuth.Static.Add(POST, "/api/login", LoginController.Login, ErrorDefaultRoute);
        preAuth.Static.Add(POST, "/api/ssologin", LoginController.LoginWithLdap, ErrorDefaultRoute);

        // Authentication middleware
        service.Routes.AuthenticateRequest = LoginController.Authenticate;

        // User model routes
        var userController = new UserController();
        string users = root + "/users";
        postAuth.Static.Add(GET, users, userController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(POST, users, userController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(GET, users + "/{userId}", userController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(PUT, users + "/{userId}", userController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(DELETE, users + "/{userId}", userController.Delete, ErrorDefaultRoute);

        // Origin model routes
        var originController = new OriginController();
        string origin = root + "/origins";
        postAuth.Static.Add(GET, origin, originController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(POST, origin, originController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(GET, origin + "/{originId}", originController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(PUT, origin + "/{originId}", originController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(DELETE, origin + "/{originId}", originController.Delete, ErrorDefaultRoute);

        // Schedule model routes
        var scheduleController = new ScheduleController();
        string schedule = root + "/schedules";
        postAuth.Static.Add(GET, schedule, scheduleController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(POST, schedule, scheduleController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(GET, schedule + "/{scheduleId}", scheduleController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(PUT, schedule + "/{scheduleId}", scheduleController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(DELETE, schedule + "/{scheduleId}", scheduleController.Delete, ErrorDefaultRoute);

        // Schedule model routes
        var destinationController = new DestinationController();
        string destination = root + "/destinations";
        postAuth.Static.Add(GET, destination, destinationController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(POST, destination, destinationController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(GET, destination + "/{destinationId}", destinationController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(PUT, destination + "/{destinationId}", destinationController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(DELETE, destination + "/{destinationId}", destinationController.Delete, ErrorDefaultRoute);

        // Extraction model routes
        var extractionController = new ExtractionController();
        string extraction = root + "/extractions";
        postAuth.Static.Add(GET, extraction, extractionController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(POST, extraction, extractionController.Post, ErrorDefaultRoute);
        postAuth.Parameter.Add(GET, extraction + "/{extractionId}", extractionController.GetById, ErrorDefaultRoute);
        postAuth.Parameter.Add(PUT, extraction + "/{extractionId}", extractionController.Put, ErrorDefaultRoute);
        postAuth.Parameter.Add(DELETE, extraction + "/{extractionId}", extractionController.Delete, ErrorDefaultRoute);
        postAuth.Parameter.Add(GET, extraction + "/execute", extractionController.ExecuteExtraction, ErrorDefaultRoute);

        // Record model routes
        var recordController = new RecordController();
        string record = root + "/records";
        postAuth.Static.Add(GET, record + "/count", recordController.GetCount, ErrorDefaultRoute);
        postAuth.Static.Add(GET, record, recordController.Get, ErrorDefaultRoute);
        postAuth.Static.Add(DELETE, record, recordController.Clear, ErrorDefaultRoute);
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