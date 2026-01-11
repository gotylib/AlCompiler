using System.Text;
using ALCompiler.CodeGenerator.Analyzer;
using ALCompiler.CodeGenerator.Models;
using ALCompiler.CodeGenerator.RegisterModel;
using ALCompiler.Parsing.Nodes;

namespace ALCompiler.CodeGenerator;

/// <summary>
/// Блочный генератор кода - сначала анализирует AST, потом генерирует код
/// </summary>
public class TaxRegisterCodeGenerator()
{
    private readonly IfBlockAnalyzer _analyzer = new();

    /// <summary>
    /// Генерирует код из AST
    /// </summary>
    public string GenerateCode(ASTNode ast)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("public bool Validate(Dictionary<string, TaxRegister> _registers)");
        sb.AppendLine("{");
        sb.AppendLine("    var isValid = true;");
        sb.AppendLine();
        
        if (ast is IfNode ifNode)
        {
            // Шаг 1: Анализируем AST и собираем данные
            var blockInfo = _analyzer.Analyze(ifNode);
            
            // Шаг 2: Генерируем код на основе собранных данных
            GenerateIfBlockCode(blockInfo, sb, indentLevel: 1);
        }
        
        sb.AppendLine();
        sb.AppendLine("    return isValid;");
        sb.AppendLine("}");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Генерирует код для блока Если-То-Иначе на основе собранных данных
    /// </summary>
    private void GenerateIfBlockCode(IfBlockInfo block, StringBuilder sb, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        
        // Определяем основной регистр из условий
        var mainRegister = block.GetConditionRegisters().FirstOrDefault() ?? "unknown";
        
        sb.AppendLine($"{indent}// Условие: {block}");
        sb.AppendLine($"{indent}{{");
        
        var innerIndent = new string(' ', (indentLevel + 1) * 4);
        
        // Генерируем выборку с условиями
        sb.AppendLine($"{innerIndent}// Выборка строк по условию");
        sb.AppendLine($"{innerIndent}var conditions = _registers[\"{mainRegister}\"].Table");
        sb.AppendLine($"{innerIndent}    .Where(row => {BuildConditionExpression(block.Conditions, mainRegister)})");
        sb.AppendLine($"{innerIndent}    .ToList();");
        sb.AppendLine();
        
        // Генерируем проверку/присваивание для Then
        if (block.ThenAssignment != null)
        {
            GenerateAssignmentValidation(block.ThenAssignment, sb, indentLevel + 1);
        }
        
        // Генерируем проверку для Else (если есть)
        if (block.ElseAssignment != null)
        {
            sb.AppendLine();
            sb.AppendLine($"{innerIndent}// Иначе");
            sb.AppendLine($"{innerIndent}var elseConditions = _registers[\"{mainRegister}\"].Table");
            sb.AppendLine($"{innerIndent}    .Where(row => !({BuildConditionExpression(block.Conditions, mainRegister)}))");
            sb.AppendLine($"{innerIndent}    .ToList();");
            sb.AppendLine();
            GenerateAssignmentValidation(block.ElseAssignment, sb, indentLevel + 1, "elseConditions");
        }
        
        sb.AppendLine($"{indent}}}");
    }
    
    /// <summary>
    /// Строит выражение условия из списка ConditionInfo
    /// </summary>
    private string BuildConditionExpression(List<ConditionInfo> conditions, string contextRegister)
    {
        if (conditions.Count == 0)
            return "true";
        
        var sb = new StringBuilder();
        
        for (var i = 0; i < conditions.Count; i++)
        {
            var cond = conditions[i];
            
            // Добавляем логический оператор (кроме первого условия)
            if (i > 0 && cond.LogicalOperator != null)
            {
                var logOp = cond.LogicalOperator == "или" ? " || " : " && ";
                sb.Append(logOp);
            }
            
            // Строим само условие
            var leftSide = cond.RegisterCode == contextRegister
                ? $"row[{cond.GraphNumber}]"
                : $"GetValue(\"{cond.RegisterCode}\", row, {cond.GraphNumber})";
            
            var rightSide = cond.IsLiteralComparison
                ? FormatLiteralValue(cond.Value)
                : $"row[{cond.CompareToGraph}]";
            
            sb.Append($"({leftSide} {cond.Operator} {rightSide})");
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Генерирует код проверки присваивания
    /// </summary>
    private static void GenerateAssignmentValidation(AssignmentInfo assignment, StringBuilder sb, int indentLevel, string conditionsVar = "conditions")
    {
        var indent = new string(' ', indentLevel * 4);
        
        if (assignment.IsLiteralAssignment)
        {
            // Присваивание литерала - просто проверяем что все значения равны литералу
            sb.AppendLine($"{indent}// Проверка: гр{assignment.TargetRegister}_{assignment.TargetGraph} должна быть {assignment.LiteralValue}");
            sb.AppendLine($"{indent}var targetValues = {conditionsVar}.Select(row => row[{assignment.TargetGraph}]);");
            sb.AppendLine($"{indent}isValid &= targetValues.All(v => Equals(v, {FormatLiteralValue(assignment.LiteralValue)}));");
        }
        else
        {
            // Присваивание из другой графы - проверяем соответствие
            sb.AppendLine($"{indent}// Проверка: значения гр{assignment.TargetRegister}_{assignment.TargetGraph} должны соответствовать гр{assignment.SourceRegister}_{assignment.SourceGraph}");
            sb.AppendLine($"{indent}var sourceValues = {conditionsVar}.Select(row => row[{assignment.SourceGraph}]).ToHashSet();");
            sb.AppendLine($"{indent}var targetValues = _registers[\"{assignment.TargetRegister}\"].Table");
            sb.AppendLine($"{indent}    .Select(row => row[{assignment.TargetGraph}]);");
            sb.AppendLine($"{indent}isValid &= targetValues.All(v => sourceValues.Contains(v));");
        }
    }
    
    /// <summary>
    /// Форматирует литеральное значение для вставки в код
    /// </summary>
    private static string FormatLiteralValue(object? value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            bool b => b ? "true" : "false",
            _ => value.ToString() ?? "null"
        };
    }
    
    /// <summary>
    /// Возвращает детальную информацию о проанализированном блоке (для отладки)
    /// </summary>
    public IfBlockInfo AnalyzeOnly(IfNode ifNode)
    {
        return _analyzer.Analyze(ifNode);
    }
}
