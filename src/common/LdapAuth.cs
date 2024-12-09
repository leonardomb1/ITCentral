using System.DirectoryServices.Protocols;
using System.Net;
using ITCentral.Types;

namespace ITCentral.Common;

public static class LdapAuth
{
    public static Result<bool, Error> AuthenticateUser(string username, string password)
    {
        try
        {
            var credential = new NetworkCredential(username, password);
            using var connection = new LdapConnection(new LdapDirectoryIdentifier(AppCommon.LdapServer))
            {
                Credential = credential,
                AuthType = AuthType.Basic
            };

            connection.Bind();

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
}