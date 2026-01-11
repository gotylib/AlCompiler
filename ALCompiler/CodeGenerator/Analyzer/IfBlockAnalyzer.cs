using ALCompiler.CodeGenerator.Models;
using ALCompiler.Lexer.Enum;
using ALCompiler.Parser;
using ALCompiler.Parser.Nodes;

namespace ALCompiler.CodeGenerator.Analyzer;

/// <summary>
/// Анализатор для сбора данных из AST блока Если-То-Иначе
/// </summary>
public class IfBlockAnalyzer
{
    private IfBlockInfo _result = null!;
    
    /// <summary>
    /// Анализирует IfNode и собирает всю информацию о блоке
    /// </summary>
    public IfBlockInfo Analyze(IfNode ifNode)
    {
        _result = new IfBlockInfo();
        
        // Проход 1: Собираем все условия
        VisitCondition(ifNode.Condition, logicalOp: null);
        
        // Проход 2: Собираем Then-блок
        if (ifNode.ThenBranch != null)
        {
            _result.ThenAssignment = VisitAssignment(ifNode.ThenBranch);
        }
        
        // Проход 3: Собираем Else-блок (если есть)
        if (ifNode.ElseBranch != null)
        {
            _result.ElseAssignment = VisitAssignment(ifNode.ElseBranch);
        }
        
        return _result;
    }
    
    /// <summary>
    /// Рекурсивно обходит условие и собирает ConditionInfo
    /// </summary>
    private void VisitCondition(ASTNode node, string? logicalOp)
    {
        switch (node)
        {
            // Логический оператор "или" - обходим обе части
            case BinaryOperationNode bin when bin.Operator.Type == TokenType.Или:
                VisitCondition(bin.Left, logicalOp);      // левая часть с текущим оператором
                VisitCondition(bin.Right, "или");         // правая часть с "или"
                break;
            
            // Логический оператор "и" - обходим обе части
            case BinaryOperationNode bin when bin.Operator.Type == TokenType.И:
                VisitCondition(bin.Left, logicalOp);      // левая часть с текущим оператором
                VisitCondition(bin.Right, "и");           // правая часть с "и"
                break;
            
            // Оператор сравнения - это конечный узел условия, СОБИРАЕМ ДАННЫЕ
            case BinaryOperationNode bin when IsComparisonOperator(bin.Operator.Type):
                var condition = new ConditionInfo
                {
                    LogicalOperator = logicalOp,
                    Operator = GetOperatorSymbol(bin.Operator.Type)
                };
                
                // Левая часть - должна быть графа
                if (bin.Left is GraphSelectorNode leftGraph)
                {
                    condition.RegisterCode = leftGraph.RegisterCode;
                    condition.GraphNumber = leftGraph.GraphNumber;
                }
                
                // Правая часть - литерал или графа
                switch (bin.Right)
                {
                    case LiteralNode literal:
                        condition.Value = literal.Value;
                        break;
                    
                    case GraphSelectorNode rightGraph:
                        condition.CompareToRegister = rightGraph.RegisterCode;
                        condition.CompareToGraph = rightGraph.GraphNumber;
                        break;
                }
                
                _result.Conditions.Add(condition);
                break;
            
            // Скобки или другие узлы - просто идём глубже
            default:
                // Если это что-то другое, пробуем как BinaryOp
                if (node is BinaryOperationNode otherBin)
                {
                    VisitCondition(otherBin.Left, logicalOp);
                    VisitCondition(otherBin.Right, logicalOp);
                }
                break;
        }
    }
    
    /// <summary>
    /// Анализирует узел присваивания
    /// </summary>
    private AssignmentInfo? VisitAssignment(ASTNode node)
    {
        if (node is not AssignmentNode assign)
            return null;
        
        var info = new AssignmentInfo
        {
            TargetRegister = assign.Target.RegisterCode,
            TargetGraph = assign.Target.GraphNumber
        };
        
        // Значение - графа или литерал
        switch (assign.Value)
        {
            case GraphSelectorNode sourceGraph:
                info.SourceRegister = sourceGraph.RegisterCode;
                info.SourceGraph = sourceGraph.GraphNumber;
                break;
            
            case LiteralNode literal:
                info.LiteralValue = literal.Value;
                break;
            
            // Если значение - выражение (например сумма), нужна дополнительная обработка
            case BinaryOperationNode binOp:
                // TODO: Обработка арифметических выражений
                info.LiteralValue = $"[выражение: {binOp.Operator.Value}]";
                break;
        }
        
        return info;
    }
    
    /// <summary>
    /// Проверяет, является ли оператор оператором сравнения
    /// </summary>
    private static bool IsComparisonOperator(TokenType type)
    {
        return type is TokenType.Equals 
            or TokenType.NotEquals 
            or TokenType.Greater 
            or TokenType.GreaterOrEqual 
            or TokenType.Less 
            or TokenType.LessOrEqual;
    }
    
    /// <summary>
    /// Возвращает символ оператора
    /// </summary>
    private static string GetOperatorSymbol(TokenType type)
    {
        return type switch
        {
            TokenType.Equals => "==",
            TokenType.NotEquals => "!=",
            TokenType.Greater => ">",
            TokenType.GreaterOrEqual => ">=",
            TokenType.Less => "<",
            TokenType.LessOrEqual => "<=",
            _ => "?"
        };
    }
}
