using System.Net;
using System.Runtime.CompilerServices;
using ITCentral.Common;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public abstract class ControllerBase
{
    protected static HttpContextBase context = new();
    protected static short BeginRequest(
        HttpContextBase ctx, 
        HttpStatusCode status, 
        [CallerMemberName] string? method = null
    )
    {
        short statusId = (short) status;
        Log.Out(
            $"{ctx.Request.Method} {statusId} - Received a request in {method} route.\n" + 
            $"  Origin - IP: {ctx.Request.Source.IpAddress}, Agent: {ctx.Request.Useragent}"
        );

        context = ctx;
        context.Response.StatusCode = statusId;

        return statusId;
    }
}