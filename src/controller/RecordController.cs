using System.Net;
using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class RecordController : ControllerBase
{
    public async Task Get(HttpContextBase ctx)
    {
        short statusId;

        var filters = ctx.Request.Query.Elements.AllKeys
            .ToDictionary(key => key ?? "", key => ctx.Request.Query.Elements[key]);

        var invalidFilters = filters.Where(f =>
            (f.Key == "relative" || f.Key == "take") &&
            !int.TryParse(f.Value, out _)).ToList();

        if (invalidFilters.Count > 0)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest, dumpLog: false);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var record = new RecordService();
        var result = await record.Get(filters);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
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

        statusId = BeginRequest(ctx, HttpStatusCode.OK, dumpLog: false);
        using Message<string> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}