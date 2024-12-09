using System.Net;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class SystemMapController : ControllerBase, IController<HttpContextBase>
{
    public async Task Get(HttpContextBase ctx)
    {       
        short statusId;

        using var systemMap = new SystemMapService();
        var result = systemMap.Get();

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if(result.Value is null) {
            statusId = BeginRequest(ctx, HttpStatusCode.OK);
            using Message<string> errMsg = new(statusId, "No Result", false);
            await context.Response.Send(errMsg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);

        using Message<SystemMap> res = new(statusId, "OK", false, result.Value);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task GetById(HttpContextBase ctx)
    {
        short statusId;

        if(!int.TryParse(ctx.Request.Url.Parameters["systemId"], null, out int systemId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        using var systemMap = new SystemMapService();
        var result = systemMap.Get(systemId);

        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if(result.Value is null) {
            statusId = BeginRequest(ctx, HttpStatusCode.OK);
            using Message<string> msg = new(statusId, "No Result", false);
            await context.Response.Send(msg.AsJsonString());
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);

        using Message<SystemMap> res = new(statusId, "OK", false, [result.Value]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Post(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<SystemMap>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var systemMap = new SystemMapService();
        var result = systemMap.Post(body.Value);

        if(!result.IsSuccessful) {
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

        var body = Converter.TryDeserializeJson<SystemMap>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        if(!int.TryParse(ctx.Request.Url.Parameters["systemId"], null, out int systemId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        using var systemMap = new SystemMapService();
        var result = systemMap.Put(body.Value, systemId);
        
        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        if(!result.Value) {
            _ = BeginRequest(ctx, HttpStatusCode.NoContent);
            await context.Response.Send("");
            return;           
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<SystemMap> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Delete(HttpContextBase ctx)
    {
        short statusId;

        if(!int.TryParse(ctx.Request.Url.Parameters["systemId"], null, out int systemId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        using var systemMap = new SystemMapService();
        var result = systemMap.Delete(systemId);
        
        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<SystemMap> res = new(statusId, "OK", false);
        await context.Response.Send(res.AsJsonString());
    }
}