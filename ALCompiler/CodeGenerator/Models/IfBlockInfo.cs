namespace ALCompiler.CodeGenerator.Models;

/// <summary>
/// Полная информация о блоке Если-То-Иначе
/// </summary>
public class IfBlockInfo
{
    /// <summary>
    /// Все условия из блока "Если ... То"
    /// </summary>
    public List<ConditionInfo> Conditions { get; } = new();
    
    /// <summary>
    /// Присваивание из блока "То"
    /// </summary>
    public AssignmentInfo? ThenAssignment { get; set; }
    
    /// <summary>
    /// Присваивание из блока "Иначе" (опционально)
    /// </summary>
    public AssignmentInfo? ElseAssignment { get; set; }
    
    /// <summary>
    /// Все уникальные регистры, участвующие в условиях
    /// </summary>
    public IEnumerable<string> GetConditionRegisters()
    {
        return Conditions
            .Select(c => c.RegisterCode)
            .Distinct();
    }
    
    /// <summary>
    /// Все уникальные графы для конкретного регистра в условиях
    /// </summary>
    public IEnumerable<int> GetConditionGraphs(string registerCode)
    {
        return Conditions
            .Where(c => c.RegisterCode == registerCode)
            .Select(c => c.GraphNumber)
            .Distinct();
    }
    
    public override string ToString()
    {
        var conditions = string.Join(" ", Conditions.Select(c => c.ToString()));
        var then = ThenAssignment?.ToString() ?? "?";
        var elseStr = ElseAssignment != null ? $" Иначе {ElseAssignment}" : "";
        return $"Если {conditions} То {then}{elseStr}";
    }
}
