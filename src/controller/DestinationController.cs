using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;
using WatsonWebserver.Core;
using static System.Net.HttpStatusCode;

namespace ITCentral.Controller;

public class DestinationController : ControllerBase, IController<HttpContextBase>
{
    public async Task Get(HttpContextBase ctx)
    {
        short statusId;

        var filters = ctx.Request.Query.Elements.AllKeys
            .ToDictionary(key => key ?? "", key => ctx.Request.Query.Elements[key]);

        using var destination = new DestinationService();
        var result = await destination.Get(filters);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, OK);

        using Message<Destination> res = new(statusId, "OK", false, result.Value);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task GetById(HttpContextBase ctx)
    {
        short statusId;

        if (!int.TryParse(ctx.Request.Url.Parameters["destinationId"], null, out int destinationId))
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var Destination = new DestinationService();
        var result = await Destination.Get(destinationId);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if (result.Value is null)
        {
            statusId = BeginRequest(ctx, OK);
            using Message<string> msg = new(statusId, "No Result", false);
            await context.Response.Send(msg.AsJsonString());
            return;
        }

        statusId = BeginRequest(ctx, OK);

        using Message<Destination> res = new(statusId, "OK", false, [result.Value]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Post(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<Destination>(ctx.Request.DataAsString);

        if (!body.IsSuccessful)
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var Destination = new DestinationService();
        var result = await Destination.Post(body.Value);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, Created);
        using Message<string> res = new(statusId, "Created", false);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Put(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<Destination>(ctx.Request.DataAsString);

        if (!body.IsSuccessful)
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        if (!int.TryParse(ctx.Request.Url.Parameters["destinationId"], null, out int destinationId))
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var Destination = new DestinationService();
        var result = await Destination.Put(body.Value, destinationId);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if (!result.Value)
        {
            _ = BeginRequest(ctx, NoContent);
            await context.Response.Send("");
            return;
        }

        statusId = BeginRequest(ctx, OK);
        using Message<Destination> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Delete(HttpContextBase ctx)
    {
        short statusId;

        if (!int.TryParse(ctx.Request.Url.Parameters["destinationId"], null, out int destinationId))
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var Destination = new DestinationService();
        var result = await Destination.Delete(destinationId);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, OK);
        using Message<Destination> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}