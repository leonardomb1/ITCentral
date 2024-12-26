using ITCentral.Common;
using LinqToDB;
using LinqToDB.Configuration;

namespace ITCentral.Data;

public class ConnectionStringSettings : IConnectionStringSettings
{
    public string ConnectionString { get; set; } = "";
    public string Name { get; set; } = "";
    public string ProviderName { get; set; } = "";
    public bool IsGlobal => false;
}

public class CallProvider : ILinqToDBSettings
{
    public IEnumerable<IDataProviderSettings> DataProviders => [];
    public string DefaultConfiguration => AppCommon.DbType;
    public string DefaultDataProvider => ProviderName.SQLite;
    public IEnumerable<IConnectionStringSettings> ConnectionStrings
    {
        get
        {
            yield return new ConnectionStringSettings
            {
                Name = ProviderName.SqlServer,
                ProviderName = ProviderName.SqlServer,
                ConnectionString = AppCommon.ConnectionString
            };

            yield return new ConnectionStringSettings
            {
                Name = ProviderName.SQLite,
                ProviderName = ProviderName.SQLite,
                ConnectionString = AppCommon.ConnectionString
            };

            yield return new ConnectionStringSettings
            {
                Name = ProviderName.PostgreSQL,
                ProviderName = ProviderName.PostgreSQL,
                ConnectionString = AppCommon.ConnectionString
            };
        }
    }
}