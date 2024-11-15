using System.Net;
using System.Reflection;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class CatController : ControllerBase, IController<HttpContextBase>
{
    public CatController()
    {
        var callerInstance = AppCommon.GenerateCallerInstance();
        Type instanceType = callerInstance.GetType();
        serviceType = typeof(CatService<>).MakeGenericType(instanceType);

        serviceInstance = Activator.CreateInstance(serviceType!, callerInstance);
    }

    public async Task Get(HttpContextBase ctx)
    {       
        short statusId;

        MethodInfo method = serviceType!.GetMethod("Read", [])!;
        var result = await (Task<Result<List<Cat>, Error>>) method.Invoke(serviceInstance, null)!;

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        List<Cat?> cats = result.Value!;

        using Message<Cat> res = new(statusId, "OK", false, [..cats!]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task GetById(HttpContextBase ctx)
    {
        short statusId;

        MethodInfo method = serviceType!.GetMethod("Read", [typeof(int)])!;

        if(!int.TryParse(ctx.Request.Url.Parameters["catId"], null, out int catId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await (Task<Result<Cat, Error>>) method.Invoke(serviceInstance, [catId])!;

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
        Cat? cat = result.Value;

        using Message<Cat> res = new(statusId, "OK", false, [ cat! ]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Post(HttpContextBase ctx)
    {
        short statusId;

        MethodInfo method = serviceType!.GetMethod("Save", [typeof(Cat)])!;

        var body = Converter.TryDeserializeJson<Cat>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        Cat cat = body.Value;
        var result = await (Task<Result<Cat, Error>>) method.Invoke(serviceInstance, [cat])!;

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.Created);
        using Message<Cat> res = new(statusId, "Created", false, [ result.Value! ]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Put(HttpContextBase ctx)
    {
        short statusId;

        MethodInfo method = serviceType!.GetMethod("Save", [typeof(Cat), typeof(int)])!;

        var body = Converter.TryDeserializeJson<Cat>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        if(!int.TryParse(ctx.Request.Url.Parameters["catId"], null, out int catId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        Cat cat = body.Value;
        var result = await (Task<Result<Cat, Error>>) method.Invoke(serviceInstance, [cat, catId])!;

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
        using Message<Cat> res = new(statusId, "OK", false, [ result.Value! ]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Delete(HttpContextBase ctx)
    {
        short statusId;

        MethodInfo method = serviceType!.GetMethod("Delete")!;

        if(!int.TryParse(ctx.Request.Url.Parameters["catId"], null, out int catId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await (Task<Result<bool, Error>>) method.Invoke(serviceInstance, [catId])!;
        
        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<Cat> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }

}