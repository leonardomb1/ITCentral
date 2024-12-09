using ITCentral.Common;
using ITCentral.Models;
using LinqToDB;
using LinqToDB.Data;

namespace ITCentral.Data;

public class CallBase : DataConnection
{   
    public CallBase() : base(AppCommon.DbType) {}
    
    public ITable<SystemMap> SystemMaps => this.GetTable<SystemMap>();
    
    public ITable<Session> Sessions => this.GetTable<Session>();
    
    public ITable<User> Users => this.GetTable<User>();
    
    public ITable<Schedule> Schedules => this.GetTable<Schedule>();
    
    public ITable<Extraction> Extractions => this.GetTable<Extraction>();
    
    public bool Exists(string tableName) 
    {
        var schemaProvider = DataProvider.GetSchemaProvider();
        var schema = schemaProvider.GetSchema(this);
        return schema.Tables.Any(t => {
            return t.TableName!.Equals(tableName, StringComparison.OrdinalIgnoreCase);
        });
    }
}