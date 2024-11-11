using System.Net;
using ITCentral.Common;
using ITCentral.Data;
using ITCentral.Models;
using ITCentral.Service;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class CatController : ControllerBase, IController<HttpContextBase>
{
    public async Task Get(HttpContextBase ctx)
    {
        short statusId;

        SqlServerCall serverCall = new(AppCommon.ConnectionString);
        CatService<SqlServerCall> catService = new(serverCall);
        var result = await catService.Read();

        if(!result.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.InternalServerError);
            using Message<Error> errMsg = new(statusId, "Internal Server Error", true, [result.Error]);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        List<Cat?> cats = result.Value;

        using Message<Cat> res = new(statusId, "OK", false, [..cats]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task GetById(HttpContextBase ctx)
    {
        short statusId;

        SqlServerCall serverCall = new(AppCommon.ConnectionString);
        CatService<SqlServerCall> catService = new(serverCall);
        
        if(!int.TryParse(ctx.Request.Url.Parameters["catId"], null, out int catId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await catService.Read(catId);

        if(!result.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.InternalServerError);
            using Message<Error> errMsg = new(statusId, "Internal Server Error", true, [result.Error]);
            await context.Response.Send(errMsg.AsJsonString());
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

        SqlServerCall serverCall = new(AppCommon.ConnectionString);
        CatService<SqlServerCall> catService = new(serverCall);

        var body = Converter.TryDeserializeJson<Cat>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        Cat cat = body.Value;
        var result = await catService.Save(cat);
        
        if(!result.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.InternalServerError);
            using Message<Error> errMsg = new(statusId, "Internal Server Error", true, [result.Error]);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.Created);
        using Message<Cat> res = new(statusId, "Created", false, [ result.Value! ]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Put(HttpContextBase ctx)
    {
        short statusId;

        SqlServerCall serverCall = new(AppCommon.ConnectionString);
        CatService<SqlServerCall> catService = new(serverCall);

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
        var result = await catService.Save(cat, catId);

        if(result.Value!.Id is null) {
            statusId = BeginRequest(ctx, HttpStatusCode.NoContent);
            using Message<string> errMsg = new(statusId, "No Content", true, []);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }
        
        if(!result.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.InternalServerError);
            using Message<Error> errMsg = new(statusId, "Internal Server Error", true, [result.Error]);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<Cat> res = new(statusId, "OK", false, [ result.Value! ]);
        await context.Response.Send(res.AsJsonString());
    }

    public async Task Delete(HttpContextBase ctx)
    {
        short statusId;

        SqlServerCall serverCall = new(AppCommon.ConnectionString);
        CatService<SqlServerCall> catService = new(serverCall);

        if(!int.TryParse(ctx.Request.Url.Parameters["catId"], null, out int catId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await catService.Delete(catId);
        
        if(!result.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.InternalServerError);
            using Message<Error> errMsg = new(statusId, "Internal Server Error", true, [result.Error]);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<Cat> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}