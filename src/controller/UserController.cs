using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;
using WatsonWebserver.Core;
using static System.Net.HttpStatusCode;

namespace ITCentral.Controller;

public class UserController : ControllerBase, IController<HttpContextBase>
{
    public async Task Get(HttpContextBase ctx)
    {
        short statusId;

        var filters = ctx.Request.Query.Elements.AllKeys
            .ToDictionary(key => key ?? "", key => ctx.Request.Query.Elements[key]);

        using var user = new UserService();
        var result = await user.Get(filters);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, OK);

        using Message<User> res = new(statusId, "OK", false, result.Value);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task GetById(HttpContextBase ctx)
    {
        short statusId;

        if (!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId))
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var user = new UserService();
        var result = await user.Get(userId);

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

        using Message<User> res = new(statusId, "OK", false, [result.Value]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Post(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if (!body.IsSuccessful)
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var user = new UserService();

        var encryptedUser = body.Value;
        encryptedUser.Password = Encryption.SymmetricEncryptAES256(body.Value.Password ?? "", AppCommon.MasterKey);
        var result = await user.Post(encryptedUser);

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

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if (!body.IsSuccessful)
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        if (!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId))
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var user = new UserService();
        var result = await user.Put(body.Value, userId);

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
        using Message<User> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Delete(HttpContextBase ctx)
    {
        short statusId;

        if (!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId))
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }


        using var user = new UserService();
        var result = await user.Delete(userId);

        if (!result.IsSuccessful)
        {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, OK);
        using Message<User> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}