using System.Net;
using ITCentral.Types;
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

    public static async Task Options(HttpContextBase ctx)
    {
        ctx.Response.Headers.Add("access-control-allow-methods", "OPTIONS, GET, POST, PUT, DELETE");
        await ctx.Response.Send();
    }

    public static async Task ErrorDefaultRoute(HttpContextBase ctx, Exception ex)
    {
        await HandleInternalServerError(ctx, new Error(ex.Message, ex.StackTrace, false));
    }
}