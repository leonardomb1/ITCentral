using System.Net;
using ITCentral.Models;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class GenericController : ControllerBase
{
    public static async Task NotFound(HttpContextBase ctx)
    {
        short statusId = BeginRequest(ctx, HttpStatusCode.NotFound);
        using Message<string> res = new(statusId, "Not Found", true);
        await context.Response.Send(res.AsJsonString());
    }

    public static async Task Test(HttpContextBase ctx)
    {
        short statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<string> res = new(statusId, "Test", false);
        await context.Response.Send(res.AsJsonString());
    }
}