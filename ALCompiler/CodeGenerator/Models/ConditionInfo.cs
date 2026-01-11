namespace ALCompiler.CodeGenerator.Models;

/// <summary>
/// Информация об одном условии сравнения
/// </summary>
public class ConditionInfo
{
    /// <summary>
    /// Код регистра (например "10102")
    /// </summary>
    public string RegisterCode { get; set; } = "";
    
    /// <summary>
    /// Номер графы (например 12)
    /// </summary>
    public int GraphNumber { get; set; }
    
    /// <summary>
    /// Оператор сравнения ("==", "!=", ">", ">=", "<", "<=")
    /// </summary>
    public string Operator { get; set; } = "==";
    
    /// <summary>
    /// Значение для сравнения (литерал)
    /// </summary>
    public object? Value { get; set; }
    
    /// <summary>
    /// Если сравнение с другой графой - код регистра
    /// </summary>
    public string? CompareToRegister { get; set; }
    
    /// <summary>
    /// Если сравнение с другой графой - номер графы
    /// </summary>
    public int? CompareToGraph { get; set; }
    
    /// <summary>
    /// Это сравнение с литералом (true) или с графой (false)
    /// </summary>
    public bool IsLiteralComparison => CompareToRegister == null;
    
    /// <summary>
    /// Логический оператор, связывающий с предыдущим условием (null, "или", "и")
    /// </summary>
    public string? LogicalOperator { get; set; }
    
    public override string ToString()
    {
        var valueStr = IsLiteralComparison 
            ? Value?.ToString() ?? "null"
            : $"гр{CompareToRegister}{CompareToGraph}";
        var logOp = LogicalOperator != null ? $"{LogicalOperator} " : "";
        return $"{logOp}гр{RegisterCode}_{GraphNumber} {Operator} {valueStr}";
    }
}
