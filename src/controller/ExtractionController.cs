using System.Net;
using ITCentral.App.Exchange;
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

        using var extraction = new ExtractionService();
        var result = await extraction.Get();

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if (result.Value is null)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.OK);
            using Message<string> errMsg = new(statusId, "No Result", false);
            await context.Response.Send(errMsg.AsJsonString());
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

    public async Task ExecuteExtractionByNameOrDestination(HttpContextBase ctx)
    {
        short statusId;

        string? queryName = ctx.Request.Query.Elements.Get("name");
        string? queryDestination = ctx.Request.Query.Elements.Get("destination");

        using var extraction = new ExtractionService();
        List<Extraction> extractions = [];

        Action switcher = (queryName, queryDestination) switch
        {
            { queryName: not null, queryDestination: not null } => async () =>
            {
                if (!int.TryParse(queryDestination, null, out int destinationId))
                {
                    statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
                    using Message<string> errMsg = new(statusId, "Bad Request", true);
                    await context.Response.Send(errMsg.AsJsonString());
                    return;
                }
                var res = await extraction.GetByNameAndDestination(destinationId, queryName);

                extractions = res.Value;
            }
            ,

            { queryName: not null, queryDestination: null } => async () =>
            {
                var res = await extraction.Get(queryName);

                extractions = res.Value;
            }
            ,

            { queryName: null, queryDestination: not null } => async () =>
            {
                if (!int.TryParse(queryDestination, null, out int destinationId))
                {
                    statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
                    using Message<string> errMsg = new(statusId, "Bad Request", true);
                    await context.Response.Send(errMsg.AsJsonString());
                    return;
                }
                var res = await extraction.GetByDestination(destinationId);

                extractions = res.Value;
            }
            ,

            _ => async () =>
            {
                statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
                using Message<string> errMsg = new(statusId, "Bad Request", true);
                await context.Response.Send(errMsg.AsJsonString());
                return;
            }
        };

        switcher.Invoke();

        extractions
            .ForEach(x =>
            {
                x.Origin!.ConnectionString = Encryption.SymmetricDecryptAES256(x.Origin!.ConnectionString, AppCommon.MasterKey);
                x.Destination!.DbString = Encryption.SymmetricDecryptAES256(x.Destination!.DbString, AppCommon.MasterKey);
            });

        var dBExchange = new MSSQLExchange();
        var result = await dBExchange.ChannelParallelize(extractions);

        if (!result.IsSuccessful)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.InternalServerError);
            using Message<Error> errMsg = new(statusId, "Extraction Failed", true, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<Extraction> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task ExecuteExtractionByScheduleId(HttpContextBase ctx)
    {
        short statusId;

        if (!int.TryParse(ctx.Request.Url.Parameters["scheduleId"], null, out int scheduleId))
        {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var extraction = new ExtractionService();
        var extractions = await extraction.GetBySchedule(scheduleId);

        extractions.Value
            .ForEach(x =>
            {
                x.Origin!.ConnectionString = Encryption.SymmetricDecryptAES256(x.Origin!.ConnectionString, AppCommon.MasterKey);
                x.Destination!.DbString = Encryption.SymmetricDecryptAES256(x.Destination!.DbString, AppCommon.MasterKey);
            });

        var dBExchange = new MSSQLExchange();
        var result = await dBExchange.ChannelParallelize(extractions.Value);

        if (!result.IsSuccessful)
        {
            statusId = BeginRequest(ctx, HttpStatusCode.InternalServerError);
            using Message<Error> errMsg = new(statusId, "Extraction Failed", true, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<Extraction> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}