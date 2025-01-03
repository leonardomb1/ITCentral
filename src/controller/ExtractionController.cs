using System.Net;
using ITCentral.App;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class ExtractionController : ControllerBase, IController<HttpContextBase>
{
    public async Task Get(HttpContextBase ctx)
    {
        short statusId;

        var filters = ctx.Request.Query.Elements.AllKeys
            .ToDictionary(key => key ?? "", key => ctx.Request.Query.Elements[key]);

        var invalidFilters = filters.Where(f =>
            (f.Key == "destination" || f.Key == "schedule" || f.Key == "origin") &&
            !int.TryParse(f.Value, out _)).ToList();

        if (invalidFilters.Count > 0)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var extraction = new ExtractionService();
        var result = await extraction.Get(filters);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);

        using Message<Extraction> res = new(statusId, "OK", false, result.Value);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task GetById(HttpContextBase ctx)
    {
        short statusId;

        if (!int.TryParse(ctx.Request.Url.Parameters["extractionId"], null, out int extractionId))
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var extraction = new ExtractionService();
        var result = await extraction.Get(extractionId);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if (result.Value is null)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.OK);
            using Message<string> msg = new(statusId, "No Result", false);
            await context.Response.Send(msg.AsJsonString());
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);

        using Message<Extraction> res = new(statusId, "OK", false, [result.Value]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Post(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<Extraction>(ctx.Request.DataAsString);

        if (!body.IsSuccessful)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var extraction = new ExtractionService();
        var result = await extraction.Post(body.Value);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.Created);
        using Message<string> res = new(statusId, "Created", false);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Put(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<Extraction>(ctx.Request.DataAsString);

        if (!body.IsSuccessful)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        if (!int.TryParse(ctx.Request.Url.Parameters["extractionId"], null, out int extractionId))
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var extraction = new ExtractionService();
        var result = await extraction.Put(body.Value, extractionId);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if (!result.Value)
        {
            _ = BeginRequest(ctx, HttpStatusCode.NoContent);
            await context.Response.Send("");
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<string> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Delete(HttpContextBase ctx)
    {
        short statusId;

        if (!int.TryParse(ctx.Request.Url.Parameters["extractionId"], null, out int extractionId))
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        var extraction = new ExtractionService();
        var result = await extraction.Delete(extractionId);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<Extraction> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task ExecuteExtraction(HttpContextBase ctx)
    {
        short statusId;

        var filters = ctx.Request.Query.Elements.AllKeys
            .ToDictionary(key => key ?? "", key => ctx.Request.Query.Elements[key]);

        var invalidFilters = filters.Where(f =>
            (f.Key == "destination" || f.Key == "schedule" || f.Key == "origin") &&
            !int.TryParse(f.Value, out _)).ToList();

        if (invalidFilters.Count > 0)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var extraction = new ExtractionService();
        var fetch = await extraction.Get(filters);

        fetch.Value
            .ForEach(x =>
            {
                x.Origin!.ConnectionString = Encryption.SymmetricDecryptAES256(x.Origin!.ConnectionString, AppCommon.MasterKey);
                x.Destination!.DbString = Encryption.SymmetricDecryptAES256(x.Destination!.DbString, AppCommon.MasterKey);
            });

        var result = await ParallelExtractionManager.ChannelParallelize(
            fetch.Value,
            ParallelExtractionManager.ProduceDBData,
            ParallelExtractionManager.ConsumeDBData

        );

        if (!result.IsSuccessful)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.InternalServerError);
            using Message<Error> errMsg = new(statusId, "Extraction Failed", true, result.Error);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<string> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}