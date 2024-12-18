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
            var credential = new NetworkCredential(username, password);
            var identifier = new LdapDirectoryIdentifier(AppCommon.LdapServer, AppCommon.LdapPort);
            using var connection = new LdapConnection(identifier, credential, AuthType.Basic);

            connection.SessionOptions.SecureSocketLayer = AppCommon.LdapSsl;
            connection.SessionOptions.VerifyServerCertificate += (conn, cert) => true;

            connection.Bind();

            string[] groups = AppCommon.LdapGroups.Split("|");
            StringBuilder stringBuilder = new();

            stringBuilder.Append($"(&(sAMAccountName={username})(|");

            foreach (string g in groups)
            {
                stringBuilder.Append($"(memberOf=CN={g},{AppCommon.LdapGroupDN})");
            }

            stringBuilder.Append("))");


            var searchRequest = new SearchRequest(
                AppCommon.LdapBaseDn,
                stringBuilder.ToString(),
                searchScope: SearchScope.Subtree,
                "sAMAccountName"
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