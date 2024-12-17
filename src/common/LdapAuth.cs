using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;
using ITCentral.Types;

namespace ITCentral.Common;

public static class LdapAuth
{
    public static Result<bool, Error> AuthenticateUser(string username, string password)
    {
        try
        {
            var credential = new NetworkCredential(username, password, AppCommon.LdapDomain);
            var identifier = new LdapDirectoryIdentifier("");
            using var connection = new LdapConnection("ldap://10.247.81.10:389");

            connection.AuthType = AuthType.Basic;
            connection.Credential = credential;

            connection.SessionOptions.SecureSocketLayer = AppCommon.LdapSsl;

            connection.Bind();

            string[] groups = AppCommon.LdapGroup.Split("|");
            StringBuilder stringBuilder = new();

            stringBuilder.Append($"(&(sAMAccountName={username})(|");

            foreach (string g in groups)
            {
                stringBuilder.Append($"(memberOf=CN={g},{AppCommon.LdapBaseDn})");
            }

            stringBuilder.Append("))");


            var searchRequest = new SearchRequest(
                AppCommon.LdapBaseDn,
                stringBuilder.ToString(),
                searchScope: SearchScope.Subtree,
                null
            );

            var searchRes = (SearchResponse)connection.SendRequest(searchRequest);

            if (searchRes.Entries.Count == 0) return AppCommon.Fail;

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
}