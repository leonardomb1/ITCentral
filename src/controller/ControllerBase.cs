using System.Net;
using System.Runtime.CompilerServices;
using ITCentral.Common;
using ITCentral.Types;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class ControllerBase
{
    protected static HttpContextBase context = new();
    protected static short BeginRequest(
        HttpContextBase ctx,
        HttpStatusCode status,
        bool dumpLog = true,
        [CallerMemberName] string? method = null
    )
    {
        short statusId = (short)status;
        Log.Out(
            $"{ctx.Request.Method} {statusId} - Received a request for {ctx.Request.Url.RawWithQuery} route.\n" +
            $"  Source - IP: {ctx.Request.RetrieveHeaderValue("X-Forwarded-For") ?? ctx.Request.Source.IpAddress} " +
            $"Agent: {ctx.Request.Useragent}, Origin: {ctx.Request.RetrieveHeaderValue("Origin")}",
            AppCommon.MessageRequest,
            dump: dumpLog
        );

        context = ctx;
        context.Response.StatusCode = statusId;

        return statusId;
    }

    protected static async Task HandleInternalServerError(HttpContextBase ctx, Error error)
    {
        Log.Out(
            $"Error occured while resolving request : {error.ExceptionMessage}" +
            $"Faulted Method was: {error.FaultedMethod}, with arguments: {error.UsedArguments}",
            AppCommon.MessageError
        );
        short statusId = BeginRequest(ctx, HttpStatusCode.InternalServerError);
        using Message<Error> errMsg = new(statusId, "Internal Server Error", true, [error]);
        await context.Response.Send(errMsg.AsJsonString());
    }
}