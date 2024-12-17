using ITCentral.Common;
using ITCentral.Models;
using LinqToDB;
using LinqToDB.Data;

namespace ITCentral.Data;

public class CallBase : DataConnection
{
    public CallBase() : base(AppCommon.DbType) { }

    public ITable<Origin> Origins => this.GetTable<Origin>();

    public ITable<Session> Sessions => this.GetTable<Session>();

    public ITable<User> Users => this.GetTable<User>();

    public ITable<Schedule> Schedules => this.GetTable<Schedule>();

    public ITable<Extraction> Extractions => this.GetTable<Extraction>();

    public ITable<Destination> Destinations => this.GetTable<Destination>();

    public ITable<Record> Records => this.GetTable<Record>();

    public bool Exists(string tableName)
    {
        var schemaProvider = DataProvider.GetSchemaProvider();
        var schema = schemaProvider.GetSchema(this);
        return schema.Tables.Any(t =>
        {
            return t.TableName!.Equals(tableName, StringComparison.OrdinalIgnoreCase);
        });
    }
}