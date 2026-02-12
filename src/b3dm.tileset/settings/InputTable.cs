namespace B3dm.Tileset;

public class InputTable
{
    public string TableName { get; set; }   
    public string GeometryColumn { get; set; }
    public string RadiusColumn { get; set; } = string.Empty;
    public string ShadersColumn { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;

    public string LodColumn { get; set; } = string.Empty;
    public string AttributeColumns { get; set; } = string.Empty;
    public int EPSGCode { get; set; }

    public string GetQueryClause()
    {
        var where = (Query != string.Empty ? $" and {Query}" : string.Empty);
        return where;
    }

}
