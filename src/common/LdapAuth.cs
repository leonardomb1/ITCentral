using System.Text;
using ITCentral.Types;
using Novell.Directory.Ldap;

namespace ITCentral.Common;

public static class LdapAuth
{
    public static Result<bool, Error> AuthenticateUser(string username, string password)
    {
        try
        {
            var connectionOptions = new LdapConnectionOptions();
            connectionOptions.ConfigureRemoteCertificateValidationCallback(
                (sender, certificate, chain, sslPolicyErrors) =>
                {
                    if (AppCommon.LdapVerifyCertificate)
                    {
                        return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
                    }
                    else
                    {
                        return true;
                    }
                }
            );

            using var connection = new LdapConnection(connectionOptions);
            connection.SecureSocketLayer = AppCommon.LdapSsl;

            connection.Connect(AppCommon.LdapServer, AppCommon.LdapPort);
            connection.Bind($"{username}@{AppCommon.LdapDomain}.com", password);

            string[] groups = AppCommon.LdapGroups.Split("|");
            StringBuilder stringBuilder = new();

            stringBuilder.Append($"(&(sAMAccountName={username})(|");

            foreach (string g in groups)
            {
                stringBuilder.Append($"(memberOf=CN={g},{AppCommon.LdapGroupDN})");
            }

            stringBuilder.Append("))");


            var results = connection.Search(
                AppCommon.LdapBaseDn,
                LdapConnection.ScopeSub,
                stringBuilder.ToString(),
                null,
                false
            );

            if (!results.HasMore())
            {
                return AppCommon.Fail;
            }
            else
            {
                return AppCommon.Success;
            }
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
}