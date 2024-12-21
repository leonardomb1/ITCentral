using ITCentral.Common;
using ITCentral.Types;
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
    public string DefaultDataProvider => AppCommon.DbType;
    public IEnumerable<IConnectionStringSettings> ConnectionStrings
    {
        get
        {
            yield return new ConnectionStringSettings
            {
                Name = DbTypes.MSSQL,
                ProviderName = ProviderName.SqlServer,
                ConnectionString = AppCommon.ConnectionString
            };

            yield return new ConnectionStringSettings
            {
                Name = DbTypes.Sqlite,
                ProviderName = ProviderName.SQLite,
                ConnectionString = AppCommon.ConnectionString
            };
        }
    }
}