namespace ALCompiler.CodeGenerator.Models;

/// <summary>
/// Информация о присваивании
/// </summary>
public class AssignmentInfo
{
    /// <summary>
    /// Целевой регистр
    /// </summary>
    public string TargetRegister { get; set; } = "";
    
    /// <summary>
    /// Целевая графа
    /// </summary>
    public int TargetGraph { get; set; }
    
    /// <summary>
    /// Исходный регистр (если значение - графа)
    /// </summary>
    public string? SourceRegister { get; set; }
    
    /// <summary>
    /// Исходная графа (если значение - графа)
    /// </summary>
    public int? SourceGraph { get; set; }
    
    /// <summary>
    /// Литеральное значение (если не графа)
    /// </summary>
    public object? LiteralValue { get; set; }
    
    /// <summary>
    /// Это присваивание литерала (true) или графы (false)
    /// </summary>
    public bool IsLiteralAssignment => SourceRegister == null;
    
    public override string ToString()
    {
        var valueStr = IsLiteralAssignment 
            ? LiteralValue?.ToString() ?? "null"
            : $"гр{SourceRegister}_{SourceGraph}";
        return $"гр{TargetRegister}_{TargetGraph} = {valueStr}";
    }
}
