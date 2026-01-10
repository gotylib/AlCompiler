using ALCompiler.Lexer;
using ALCompiler.Lexer.Enum;
using ALCompiler.Parser.Exception;
using ALCompiler.Parser.Nodes;

namespace ALCompiler.Parser;

public class Parser(List<Token> tokens)
{
    private int _position = 0;

    public ASTNode Parse()
    {
        if (Match(TokenType.Если))
        {
            return ParseIfStatement();
        }
        
        if (Peek().Type == TokenType.Identifier && 
            Peek().Value.StartsWith("гр", StringComparison.OrdinalIgnoreCase))
        {
            return ParseAssignmentOrOperation();
        }
        
        throw new ParseException($"Неожиданный токен: {Peek().Value}");
    }
    
    private ASTNode ParseIfStatement()
    {
        var condition = ParseExpression();
        
        Consume(TokenType.То, "Ожидается 'то' после условия");
        
        var thenBranch = ParseAssignmentOrOperation();
        
        ASTNode elseBranch = null;
        if (Match(TokenType.Иначе))
        {
            elseBranch = ParseAssignmentOrOperation();
        }
        
        return new IfNode(condition, thenBranch, elseBranch);
    }
    
    private ASTNode ParseAssignmentOrOperation()
    {
        var left = ParseGraphSelector();
        
        if (Match(TokenType.Assign))
        {
            // Обычное присваивание
            var value = ParseExpression();
            return new AssignmentNode(left, value);
        }
        else if (Match(TokenType.Equals))
        {
            // Проверка равенства (в условии)
            var right = ParseExpression();
            return new BinaryOperationNode(left, Previous(), right);
        }
        else
        {
            throw new ParseException($"Ожидается '=' или '==' после графы");
        }
    }
    
    private ASTNode ParseExpression()
    {
        return ParseLogicalOr();
    }
    
    private ASTNode ParseLogicalOr()
    {
        var expr = ParseLogicalAnd();
        
        while (Match(TokenType.Или))
        {
            var op = Previous();
            var right = ParseLogicalAnd();
            expr = new BinaryOperationNode(expr, op, right);
        }
        
        return expr;
    }
    
    private ASTNode ParseLogicalAnd()
    {
        var expr = ParseComparison();
        
        while (Match(TokenType.И))
        {
            var op = Previous();
            var right = ParseComparison();
            expr = new BinaryOperationNode(expr, op, right);
        }
        
        return expr;
    }
    
    private ASTNode ParseComparison()
    {
        var expr = ParseGraphSelectorOrValue();
        
        while (Match(TokenType.Equals, TokenType.NotEquals, 
                    TokenType.Greater, TokenType.GreaterOrEqual,
                    TokenType.Less, TokenType.LessOrEqual))
        {
            var op = Previous();
            var right = ParseGraphSelectorOrValue();
            expr = new BinaryOperationNode(expr, op, right);
        }
        
        return expr;
    }
    
    private ASTNode ParseGraphSelectorOrValue()
    {
        if (Peek().Type == TokenType.Identifier && 
            Peek().Value.StartsWith("гр", StringComparison.OrdinalIgnoreCase))
        {
            return ParseGraphSelector();
        }
        
        if (Match(TokenType.Number))
        {
            return new LiteralNode(double.Parse(Previous().Value));
        }
        
        if (Match(TokenType.String))
        {
            return new LiteralNode(Previous().Value);
        }
        
        if (Match(TokenType.Пусто))
        {
            return new LiteralNode(null);
        }
        
        if (Match(TokenType.LParen))
        {
            var expr = ParseExpression();
            Consume(TokenType.RParen, "Ожидается ')'");
            return expr;
        }
        
        throw new ParseException($"Неожиданный токен: {Peek().Value}");
    }
    
    private GraphSelectorNode ParseGraphSelector()
    {
        var token = Consume(TokenType.Identifier, "Ожидается идентификатор графы");
        return new GraphSelectorNode(token.Value);
    }
    
    // Вспомогательные методы
    private bool Match(params TokenType[] types)
    {
        if (!types.Any(Check)) return false;
        Advance();
        return true;

    }
    
    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }
    
    private Token Advance()
    {
        if (!IsAtEnd()) _position++;
        return Previous();
    }
    
    private bool IsAtEnd() => Peek().Type == TokenType.EOF;
    private Token Peek() => tokens[_position];
    private Token Previous() => tokens[_position - 1];
    
    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new ParseException(message);
    }
}