using System.Net;
using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class RecordController : ControllerBase
{
    public async Task GetLast(HttpContextBase ctx)
    {
        short statusId;

        if (!int.TryParse(ctx.Request.RetrieveQueryValue("filterTimeInSecs"), null, out int filterTimeInSecs))
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest, dumpLog: false);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var record = new RecordService();
        var result = await record.GetLast(filterTimeInSecs);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if (result.Value is null)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.OK, dumpLog: false);
            using Message<string> errMsg = new(statusId, "No Result", false);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK, dumpLog: false);

        using Message<Record> res = new(statusId, "OK", false, result.Value);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task GetCount(HttpContextBase ctx)
    {
        short statusId;

        using var record = new RecordService();
        var result = await record.GetCount();

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK, dumpLog: false);

        using Message<string> res = new(statusId, "OK", false, [$"Record count is: {result.Value}"]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Clear(HttpContextBase ctx)
    {
        short statusId;

        using var record = new RecordService();
        var result = await record.Clear();

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<string> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}