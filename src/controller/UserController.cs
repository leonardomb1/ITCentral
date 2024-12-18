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
            statusId = BeginRequest(ctx, HttpStatusCode.OK);
            using Message<string> errMsg = new(statusId, "No Result", false, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);

        using Message<User> res = new(statusId, "OK", false, result.Value);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task GetById(HttpContextBase ctx)
    {
        short statusId;

        if(!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await new UserService().GetById(userId);

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if(result.Value is null) {
            statusId = BeginRequest(ctx, HttpStatusCode.OK);
            using Message<string> msg = new(statusId, "No Result", false, null!);
            await context.Response.Send(msg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);

        using Message<User> res = new(statusId, "OK", false, [result.Value]);
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

        if(result.Value.Count == 0) {
            statusId = BeginRequest(ctx, HttpStatusCode.OK);
            using Message<string> msg = new(statusId, "No Result", false, null!);
            await context.Response.Send(msg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);

        using Message<User> res = new(statusId, "OK", false, result.Value);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Login(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if(!body.IsSuccessful || body.Value.Name == "" || body.Value.Password == "") {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        var userSecret = await new UserService().GetUserCredential(body.Value.Name!);

        if(!userSecret.IsSuccessful) {
            await HandleInternalServerError(ctx, userSecret.Error);
            return;
        }

        if(userSecret.Value is null || userSecret.Value == "" || userSecret.Value != body.Value.Password) {
            statusId = BeginRequest(ctx, HttpStatusCode.Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<string> res = new(statusId, "OK", false, [SessionManager.CreateSession(ctx.Request.Source.IpAddress).sessionId]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Post(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        var encryptedUser = body.Value;
        encryptedUser.Password = Encryption.SymmetricEncryptAES256(body.Value.Password ?? "", AppCommon.MasterKey);
        var result = await new UserService().Post(encryptedUser);

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.Created);
        using Message<string> res = new(statusId, "Created", false, null!);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Put(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        if(!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await new UserService().Put(body.Value, userId);

        if(result.Value is null) {
            _ = BeginRequest(ctx, HttpStatusCode.NoContent);
            await context.Response.Send("");
            return;           
        }
        
        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<User> res = new(statusId, "OK", false, [result.Value]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Delete(HttpContextBase ctx)
    {
        short statusId;

        if(!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await new UserService().Delete(userId);
        
        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<User> res = new(statusId, "OK", false, null!);
        await context.Response.Send(res.AsJsonString());
    }
}