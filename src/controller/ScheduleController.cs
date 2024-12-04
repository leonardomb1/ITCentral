using System.Net;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Service;
using WatsonWebserver.Core;

namespace ITCentral.Controller;

public class ScheduleController : ControllerBase, IController<HttpContextBase>
{
    public async Task Get(HttpContextBase ctx)
    {       
        short statusId;

        var result = await new ScheduleService().Get();

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

        using Message<Schedule> res = new(statusId, "OK", false, result.Value);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task GetById(HttpContextBase ctx)
    {
        short statusId;

        if(!int.TryParse(ctx.Request.Url.Parameters["scheduleId"], null, out int scheduleId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await new ScheduleService().GetById(scheduleId);

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

        using Message<Schedule> res = new(statusId, "OK", false, [result.Value]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Post(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<Schedule>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        var result = await new ScheduleService().Post(body.Value);

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

        var body = Converter.TryDeserializeJson<Schedule>(ctx.Request.DataAsString);

        if(!body.IsSuccessful) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        if(!int.TryParse(ctx.Request.Url.Parameters["scheduleId"], null, out int scheduleId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await new ScheduleService().Put(body.Value, scheduleId);

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
        using Message<Schedule> res = new(statusId, "OK", false, [result.Value]);
        await context.Response.Send(res.AsJsonString());
    }
    public async Task Delete(HttpContextBase ctx)
    {
        short statusId;

        if(!int.TryParse(ctx.Request.Url.Parameters["scheduleId"], null, out int scheduleId)) {
            statusId = BeginRequest(ctx, HttpStatusCode.BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true, null!);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        } 

        var result = await new ScheduleService().Delete(scheduleId);
        
        if(!result.IsSuccessful) {
            await HandleInternalServerError(ctx, result.Error);
            return;
        }

        statusId = BeginRequest(ctx, HttpStatusCode.OK);
        using Message<Schedule> res = new(statusId, "OK", false, null!);
        await context.Response.Send(res.AsJsonString());
    }
}