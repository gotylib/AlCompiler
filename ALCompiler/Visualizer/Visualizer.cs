using System.Text;
using ALCompiler.Lexing;
using ALCompiler.Parsing.Nodes;

namespace ALCompiler.Visualization;

public static class Visualizer
{
    #region Lexer Visualization
    
    /// <summary>
    /// Визуализирует список токенов в виде таблицы
    /// </summary>
    public static void PrintTokens(List<Token> tokens)
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        const int typeWidth = 16;
        const int valueWidth = 20;
        const int lineWidth = 6;
        const int posWidth = 6;
        
        var totalWidth = typeWidth + valueWidth + lineWidth + posWidth + 13;
        
        // Заголовок
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', totalWidth));
        Console.WriteLine($"║ {"Тип",-typeWidth} │ {"Значение",-valueWidth} │ {"Стр",-lineWidth} │ {"Поз",-posWidth} ║");
        Console.WriteLine(new string('═', totalWidth));
        Console.ResetColor();
        
        // Токены
        foreach (var token in tokens)
        {
            var typeColor = GetTokenColor(token.Type);
            var displayValue = TruncateString(token.Value ?? "null", valueWidth);
            
            Console.Write("║ ");
            Console.ForegroundColor = typeColor;
            Console.Write($"{token.Type,-typeWidth}");
            Console.ResetColor();
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{displayValue,-valueWidth}");
            Console.ResetColor();
            Console.Write($" │ {token.Line,-lineWidth} │ {token.Position,-posWidth} ║");
            Console.WriteLine();
        }
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', totalWidth));
        Console.ResetColor();
        
        Console.WriteLine($"\nВсего токенов: {tokens.Count}");
    }
    
    private static ConsoleColor GetTokenColor(Lexing.Enum.TokenType type)
    {
        return type switch
        {
            Lexing.Enum.TokenType.Если or 
            Lexing.Enum.TokenType.То or 
            Lexing.Enum.TokenType.Иначе => ConsoleColor.Magenta,
            
            Lexing.Enum.TokenType.И or 
            Lexing.Enum.TokenType.Или or 
            Lexing.Enum.TokenType.Не => ConsoleColor.Blue,
            
            Lexing.Enum.TokenType.Number => ConsoleColor.Green,
            Lexing.Enum.TokenType.String => ConsoleColor.DarkYellow,
            Lexing.Enum.TokenType.Identifier => ConsoleColor.White,
            
            Lexing.Enum.TokenType.Equals or
            Lexing.Enum.TokenType.NotEquals or
            Lexing.Enum.TokenType.Greater or
            Lexing.Enum.TokenType.GreaterOrEqual or
            Lexing.Enum.TokenType.Less or
            Lexing.Enum.TokenType.LessOrEqual or
            Lexing.Enum.TokenType.Assign => ConsoleColor.Red,
            
            Lexing.Enum.TokenType.EOF => ConsoleColor.DarkGray,
            Lexing.Enum.TokenType.Invalid => ConsoleColor.DarkRed,
            
            _ => ConsoleColor.Gray
        };
    }
    
    #endregion
    
    #region Parser Visualization
    
    /// <summary>
    /// Визуализирует AST-дерево в консоль
    /// </summary>
    public static void PrintAST(ASTNode node, string title = "AST")
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n╔════ {title} ════╗");
        Console.ResetColor();
        
        PrintNode(node, "", true);
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╚" + new string('═', title.Length + 10) + "╝\n");
        Console.ResetColor();
    }
    
    private static void PrintNode(ASTNode? node, string indent, bool isLast)
    {
        if (node == null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"{indent}{(isLast ? "└── " : "├── ")}null");
            Console.ResetColor();
            return;
        }
        
        var marker = isLast ? "└── " : "├── ";
        var childIndent = indent + (isLast ? "    " : "│   ");
        
        switch (node)
        {
            case IfNode ifNode:
                PrintIfNode(ifNode, indent, marker, childIndent);
                break;
                
            case BinaryOperationNode binOp:
                PrintBinaryOperationNode(binOp, indent, marker, childIndent);
                break;
                
            case AssignmentNode assignment:
                PrintAssignmentNode(assignment, indent, marker, childIndent);
                break;
                
            case GraphSelectorNode graph:
                PrintGraphSelectorNode(graph, indent, marker);
                break;
                
            case LiteralNode literal:
                PrintLiteralNode(literal, indent, marker);
                break;
                
            default:
                Console.WriteLine($"{indent}{marker}{node.GetType().Name}");
                break;
        }
    }
    
    private static void PrintIfNode(IfNode node, string indent, string marker, string childIndent)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"{indent}{marker}Если");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"{childIndent}├── [Условие]");
        Console.ResetColor();
        PrintNode(node.Condition, childIndent + "│   ", false);
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"{childIndent}├── [То]");
        Console.ResetColor();
        PrintNode(node.ThenBranch, childIndent + "│   ", false);
        
        if (node.ElseBranch != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"{childIndent}└── [Иначе]");
            Console.ResetColor();
            PrintNode(node.ElseBranch, childIndent + "    ", true);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"{childIndent}└── [Иначе: отсутствует]");
            Console.ResetColor();
        }
    }
    
    private static void PrintBinaryOperationNode(BinaryOperationNode node, string indent, string marker, string childIndent)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{indent}{marker}Операция: {node.Operator.Value} ({node.Operator.Type})");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"{childIndent}├── [Левый операнд]");
        Console.ResetColor();
        PrintNode(node.Left, childIndent + "│   ", false);
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"{childIndent}└── [Правый операнд]");
        Console.ResetColor();
        PrintNode(node.Right, childIndent + "    ", true);
    }
    
    private static void PrintAssignmentNode(AssignmentNode node, string indent, string marker, string childIndent)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{indent}{marker}Присваивание");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"{childIndent}├── [Цель]");
        Console.ResetColor();
        PrintNode(node.Target, childIndent + "│   ", false);
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"{childIndent}└── [Значение]");
        Console.ResetColor();
        PrintNode(node.Value, childIndent + "    ", true);
    }
    
    private static void PrintGraphSelectorNode(GraphSelectorNode node, string indent, string marker)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{indent}{marker}Графа: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"гр{node.RegisterCode}{node.TablePart}{node.GraphNumber:D2}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"{indent}       (Регистр: {node.RegisterCode}, Табл.часть: {node.TablePart}, Графа: {node.GraphNumber})");
        Console.ResetColor();
    }
    
    private static void PrintLiteralNode(LiteralNode node, string indent, string marker)
    {
        var valueStr = node.Value?.ToString() ?? "null";
        var typeStr = node.Value?.GetType().Name ?? "null";
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"{indent}{marker}Литерал: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(valueStr);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($" ({typeStr})");
        Console.ResetColor();
    }
    
    #endregion
    
    #region Helpers
    
    private static string TruncateString(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return str.Length <= maxLength ? str : str[..(maxLength - 3)] + "...";
    }
    
    /// <summary>
    /// Возвращает строковое представление AST-дерева
    /// </summary>
    public static string GetASTString(ASTNode node)
    {
        var sb = new StringBuilder();
        BuildASTString(node, sb, "", true);
        return sb.ToString();
    }
    
    private static void BuildASTString(ASTNode? node, StringBuilder sb, string indent, bool isLast)
    {
        if (node == null)
        {
            sb.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}null");
            return;
        }
        
        var marker = isLast ? "└── " : "├── ";
        var childIndent = indent + (isLast ? "    " : "│   ");
        
        switch (node)
        {
            case IfNode ifNode:
                sb.AppendLine($"{indent}{marker}Если");
                sb.AppendLine($"{childIndent}├── [Условие]");
                BuildASTString(ifNode.Condition, sb, childIndent + "│   ", false);
                sb.AppendLine($"{childIndent}├── [То]");
                BuildASTString(ifNode.ThenBranch, sb, childIndent + "│   ", false);
                if (ifNode.ElseBranch != null)
                {
                    sb.AppendLine($"{childIndent}└── [Иначе]");
                    BuildASTString(ifNode.ElseBranch, sb, childIndent + "    ", true);
                }
                break;
                
            case BinaryOperationNode binOp:
                sb.AppendLine($"{indent}{marker}Операция: {binOp.Operator.Value}");
                sb.AppendLine($"{childIndent}├── [Левый]");
                BuildASTString(binOp.Left, sb, childIndent + "│   ", false);
                sb.AppendLine($"{childIndent}└── [Правый]");
                BuildASTString(binOp.Right, sb, childIndent + "    ", true);
                break;
                
            case AssignmentNode assignment:
                sb.AppendLine($"{indent}{marker}Присваивание");
                sb.AppendLine($"{childIndent}├── [Цель]");
                BuildASTString(assignment.Target, sb, childIndent + "│   ", false);
                sb.AppendLine($"{childIndent}└── [Значение]");
                BuildASTString(assignment.Value, sb, childIndent + "    ", true);
                break;
                
            case GraphSelectorNode graph:
                sb.AppendLine($"{indent}{marker}Графа: гр{graph.RegisterCode}{graph.TablePart}{graph.GraphNumber:D2}");
                break;
                
            case LiteralNode literal:
                sb.AppendLine($"{indent}{marker}Литерал: {literal.Value}");
                break;
                
            default:
                sb.AppendLine($"{indent}{marker}{node.GetType().Name}");
                break;
        }
    }
    
    #endregion
}
