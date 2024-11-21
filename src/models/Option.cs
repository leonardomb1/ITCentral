using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Models;

[Table("OPTIONS")]
public class Option
{
    public int? Id {get; set;}
    public string? FilterColumn {get; set;}
    public int? FilterTime {get; set;}
    public string? IndexName {get; set;}
    public Option() {}
    public Option(
        string filterColumn,
        int filterTime,
        string indexName
    ) {
        FilterColumn = filterColumn;
        FilterTime = filterTime;
        IndexName = indexName;
    }
}