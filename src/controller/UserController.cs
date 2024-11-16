using System.Net;
using ITCentral.App;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Service;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class UserController : ControllerBase, IController<HttpContextBase>
{
    public async Task Get(HttpContextBase ctx)
    {       
        short statusId;

        var result = await new UserService().Get();

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if(result.Value is null) {
            statusId = BeginRequest(ctx, HttpStatusCode.NoContent);
            using Message<string> errMsg = new(statusId, "No Content", true, []);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        List<User?> users = result.Value!;

        using Message<User> res = new(statusId, "OK", false, [..users!]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task GetById(HttpContextBase ctx)
    {
        short statusId;

        if(!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await new UserService().GetById(userId);

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if(result.Value!.Id is null) {
            statusId = BeginRequest(ctx, HttpStatusCode.NoContent);
            using Message<string> errMsg = new(statusId, "No Content", true, []);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        User user = result.Value;

        using Message<User> res = new(statusId, "OK", false, [ user! ]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task GetByName(HttpContextBase ctx)
    {
        short statusId;

        string name = ctx.Request.Url.Parameters["userName"]!;

        var result = await new UserService().GetByName(name);

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if(result.Value is null) {
            statusId = BeginRequest(ctx, HttpStatusCode.NoContent);
            using Message<string> errMsg = new(statusId, "No Content", true, []);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        User user = result.Value[0];

        using Message<User> res = new(statusId, "OK", false, [ user! ]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Login(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        var userSecret = await new UserService().GetUserCredential(body.Value.Name!);

        if(!userSecret.IsSuccessful) {
            await HandleInternalServerError(ctx, userSecret.Error);
            return;
        }

        if(userSecret.Value is null || userSecret.Value != body.Value.Password) {
            statusId = BeginRequest(ctx, HttpStatusCode.Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true, []);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<string> res = new(statusId, "OK", false, [ SessionManager.CreateSession(ctx.Request.Source.IpAddress).sessionId ]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Post(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        var result = await new UserService().Post(body.Value);

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.Created);
        using Message<User> res = new(statusId, "Created", false, [ result.Value ]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Put(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        if(!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await new UserService().Put(body.Value, userId);

        if(result.Value!.Id is null) {
            statusId = BeginRequest(ctx, HttpStatusCode.NoContent);
            using Message<string> errMsg = new(statusId, "No Content", true, []);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }
        
        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<User> res = new(statusId, "OK", false, [ result.Value ]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Delete(HttpContextBase ctx)
    {
        short statusId;

        if(!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await new UserService().Delete(userId);
        
        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<User> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}