namespace ITCentral.Types;

public struct DbTypes
{
    public const string PostgreSQL = "PostgreSQL";
    public const string MySQL = "MySQL";
    public const string MSSQL = "MSSQL";
    public const string Sqlite = "Sqlite";
    public const string Clickhouse = "Clickhouse";

    public static readonly string[] All = [PostgreSQL, MySQL, MSSQL];
}