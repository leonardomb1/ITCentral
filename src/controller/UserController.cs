using System.Net;
using System.Reflection;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class UserController : ControllerBase, IController<HttpContextBase>
{
    public UserController()
    {
        var callerInstance = AppCommon.GenerateCallerInstance();
        Type instanceType = callerInstance.GetType();
        serviceType = typeof(UserService<>).MakeGenericType(instanceType);

        serviceInstance = Activator.CreateInstance(serviceType!, callerInstance);
    }
    public async Task Get(HttpContextBase ctx)
    {       
        short statusId;

        MethodInfo method = serviceType!.GetMethod("Read", [])!;
        var result = await (Task<Result<List<User>, Error>>) method.Invoke(serviceInstance, null)!;

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

        MethodInfo method = serviceType!.GetMethod("Read", [typeof(int)])!;

        if(!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await (Task<Result<User, Error>>) method.Invoke(serviceInstance, [userId])!;

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

        MethodInfo method = serviceType!.GetMethod("Read", [typeof(string)])!;

        string name = ctx.Request.Url.Parameters["userName"]!;

        var result = await (Task<Result<User, Error>>) method.Invoke(serviceInstance, [name])!;

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
    public async Task GetSessionId(HttpContextBase ctx)
    {
        short statusId;

        MethodInfo method = serviceType!.GetMethod("Read", [typeof(string)])!;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        var result = await (Task<Result<List<User>, Error>>) method.Invoke(serviceInstance, [body.Value.Name])!;
        var userSecret = Encryption.SymmetricDecryptAES256(result.Value[0].Password!, AppCommon.MasterKey);

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if(result.Value![0].Id is null || body.Value.Password != userSecret) {
            statusId = BeginRequest(ctx, HttpStatusCode.Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true, []);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<string> res = new(statusId, "OK", false, [ AppCommon.GenerateSessionId(ctx.Request.Source.IpAddress) ]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Post(HttpContextBase ctx)
    {
        short statusId;

        MethodInfo method = serviceType!.GetMethod("Save", [typeof(User)])!;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        User encryptedUser = body.Value;
        encryptedUser.Password = Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);

        var result = await (Task<Result<User, Error>>) method.Invoke(serviceInstance, [encryptedUser])!;

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

        MethodInfo method = serviceType!.GetMethod("Save", [typeof(User), typeof(int)])!;

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

        User encryptedUser = body.Value;
        encryptedUser.Password = Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);
        var result = await (Task<Result<User, Error>>) method.Invoke(serviceInstance, [encryptedUser, userId])!;

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

        MethodInfo method = serviceType!.GetMethod("Delete")!;

        if(!int.TryParse(ctx.Request.Url.Parameters["userId"], null, out int userId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await (Task<Result<bool, Error>>) method.Invoke(serviceInstance, [userId])!;
        
        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<User> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}