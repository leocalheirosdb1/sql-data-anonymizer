namespace SqlDataAnonymizer.Domain.DTO;

public sealed class SensitiveColumnDto
{
    public string Schema { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string SensitiveType { get; set; } = string.Empty;
    
    public string FullTableName(string quoteChar = "[")
    {
        var closeQuote = quoteChar == "[" ? "]" : quoteChar;
        
        if (string.IsNullOrEmpty(Schema))
            return $"{quoteChar}{TableName}{closeQuote}";
            
        return $"{quoteChar}{Schema}{closeQuote}. {quoteChar}{TableName}{closeQuote}";
    }
}