using System.Net;
using ITCentral.App;
using ITCentral.Common;
using ITCentral.Models;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class GenericController : ControllerBase
{
    public static async Task NotFound(HttpContextBase ctx)
    {
        short statusId = BeginRequest(ctx, HttpStatusCode.NotFound);
        using Message<string> res = new(statusId, "Not Found", true, null!);
        await context.Response.Send(res.AsJsonString());
    }
    public static async Task Options(HttpContextBase ctx)
    {
        ctx.Response.Headers.Add("Allow", "OPTIONS, GET, POST, PUT, DELETE");
        await ctx.Response.Send();
    }
    public static async Task Authenticate(HttpContextBase ctx)
    {
        if(ctx.Request.Url.RawWithoutQuery == "/api/users/login") return;
        if(ctx.Request.HeaderExists("Key"))
        {
            if(ctx.Request.Headers.Get("Key") == AppCommon.ApiKey) return;
        }
        if(!ctx.Request.HeaderExists("Authorization"))
        {
            short statusId = BeginRequest(ctx, HttpStatusCode.Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        string sessionId = ctx.Request.Headers.Get("Authorization")!;

        if(!SessionManager.IsSessionValid(sessionId))
        {
            short statusId = BeginRequest(ctx, HttpStatusCode.Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }
    }
    public static async Task ErrorDefaultRoute(HttpContextBase ctx, Exception ex)
    {
        await HandleInternalServerError(ctx, new Error(ex.Message, ex.StackTrace, false));
    }
}