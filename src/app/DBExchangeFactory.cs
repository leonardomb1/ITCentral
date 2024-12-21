using ITCentral.Types;

namespace ITCentral.App;

public static class DBExchangeFactory
{
    public static DBExchange Create(string type)
    {
        return type switch
        {
            DbTypes.PostgreSQL => new PostgreSQLExchange(),
            DbTypes.MySQL => new MySQLExchange(),
            DbTypes.MSSQL => new MSSQLExchange(),
            DbTypes.Clickhouse => new ClickhouseExchange(),
            _ => throw new NotSupportedException($"Database type '{type}' is not supported")
        };
    }
}