using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Service;
using ITCentral.Types;
using WatsonWebserver.Core;
using static System.Net.HttpStatusCode;

namespace ITCentral.Controller;

public class LoginController : ControllerBase
{
    public static async Task Login(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if (!body.IsSuccessful || body.Value.Name == "" || body.Value.Password == "")
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        using var user = new UserService();
        var userSecret = await user.GetUserCredential(body.Value.Name!);

        if (!userSecret.IsSuccessful)
        {
            await HandleInternalServerError(ctx, userSecret.Error);
            return;
        }

        if (userSecret.Value is null || userSecret.Value == "" || userSecret.Value != body.Value.Password)
        {
            statusId = BeginRequest(ctx, Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        string jwt = Encryption.GenerateJwt(ctx.Request.Source.IpAddress, AppCommon.MasterKey);

        statusId = BeginRequest(ctx, OK);
        using Message<object> res = new(statusId, "OK", false, [new { jwt }]);
        await context.Response.Send(res.AsJsonString());
    }

    public static async Task LoginWithLdap(HttpContextBase ctx)
    {
        short statusId;

        var body = Converter.TryDeserializeJson<User>(ctx.Request.DataAsString);

        if (!body.IsSuccessful || body.Value.Name == "" || body.Value.Password == "")
        {
            statusId = BeginRequest(ctx, BadRequest);
            using Message<string> errMsg = new(statusId, "Bad Request", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        var ldapSearch = LdapAuth.AuthenticateUser(body.Value.Name, body.Value.Password!);

        if (!ldapSearch.IsSuccessful)
        {
            statusId = BeginRequest(ctx, Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        if (!ldapSearch.Value)
        {
            statusId = BeginRequest(ctx, Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        string jwt = Encryption.GenerateJwt(ctx.Request.Source.IpAddress, AppCommon.MasterKey);

        statusId = BeginRequest(ctx, OK);
        using Message<object> res = new(statusId, "OK", false, [new { jwt }]);
        await context.Response.Send(res.AsJsonString());
    }

    public static async Task Authenticate(HttpContextBase ctx)
    {
        if (
            ctx.Request.Url.RawWithoutQuery == "/api/login" ||
            ctx.Request.Url.RawWithoutQuery == "/api/ssologin"
        ) return;

        if (ctx.Request.HeaderExists("Key"))
        {
            if (ctx.Request.Headers.Get("Key") == AppCommon.ApiKey) return;
        }

        if (!ctx.Request.HeaderExists("Authorization"))
        {
            short statusId = BeginRequest(ctx, Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }

        string jwt = ctx.Request.Headers.Get("Authorization")!;

        if (!Encryption.ValidateJwt(ctx.Request.Source.IpAddress, jwt, AppCommon.MasterKey).IsSuccessful)
        {
            short statusId = BeginRequest(ctx, Unauthorized);
            using Message<string> errMsg = new(statusId, "Unauthorized", true);
            await context.Response.Send(errMsg.AsJsonString());
            return;
        }
    }
}