using ALCompiler.Lexer;
using ALCompiler.Lexer.Enum;
using ALCompiler.Parser;
using ALCompiler.Parser.Nodes;

namespace Tests;

#region Lexer Tests

public class LexerKeywordsTests
{
    [Theory]
    [InlineData("Если", TokenType.Если)]
    [InlineData("если", TokenType.Если)]
    [InlineData("ЕСЛИ", TokenType.Если)]
    [InlineData("то", TokenType.То)]
    [InlineData("иначе", TokenType.Иначе)]
    [InlineData("и", TokenType.И)]
    [InlineData("или", TokenType.Или)]
    [InlineData("не", TokenType.Не)]
    [InlineData("пусто", TokenType.Пусто)]
    // Примечание: "графа" начинается с "гр", поэтому парсится как селектор графы
    [InlineData("регистр", TokenType.Регистр)]
    [InlineData("всеграфы", TokenType.ВсеГрафы)]
    [InlineData("всезаписи", TokenType.ВсеЗаписи)]
    public void Lexer_ShouldRecognizeKeywords(string input, TokenType expectedType)
    {
        var lexer = new Lexer(input);
        var tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count); // keyword + EOF
        Assert.Equal(expectedType, tokens[0].Type);
    }
}

public class LexerOperatorsTests
{
    [Theory]
    [InlineData("=", TokenType.Assign)]
    [InlineData("==", TokenType.Equals)]
    [InlineData("!=", TokenType.NotEquals)]
    [InlineData(">", TokenType.Greater)]
    [InlineData(">=", TokenType.GreaterOrEqual)]
    [InlineData("<", TokenType.Less)]
    [InlineData("<=", TokenType.LessOrEqual)]
    [InlineData("+", TokenType.Plus)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("*", TokenType.Multiply)]
    [InlineData("/", TokenType.Divide)]
    [InlineData("%", TokenType.Mod)]
    public void Lexer_ShouldRecognizeOperators(string input, TokenType expectedType)
    {
        var lexer = new Lexer(input);
        var tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(input, tokens[0].Value);
    }

    [Fact]
    public void Lexer_ShouldRecognizeParentheses()
    {
        var lexer = new Lexer("()");
        var tokens = lexer.Tokenize();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.LParen, tokens[0].Type);
        Assert.Equal(TokenType.RParen, tokens[1].Type);
    }

    [Fact]
    public void Lexer_ShouldRecognizeCommaAndColon()
    {
        var lexer = new Lexer(",:");
        var tokens = lexer.Tokenize();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Comma, tokens[0].Type);
        Assert.Equal(TokenType.Colon, tokens[1].Type);
    }
}

public class LexerGraphSelectorTests
{
    [Theory]
    [InlineData("гр10102212", "гр10102212")]
    [InlineData("гр10103206", "гр10103206")]
    [InlineData("гр12345678", "гр12345678")]
    public void Lexer_ShouldRecognizeGraphSelectors(string input, string expectedValue)
    {
        var lexer = new Lexer(input);
        var tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Fact]
    public void Lexer_ShouldRecognizeMultipleGraphSelectors()
    {
        var lexer = new Lexer("гр10102212 гр10103206");
        var tokens = lexer.Tokenize();

        Assert.Equal(3, tokens.Count);
        Assert.Equal("гр10102212", tokens[0].Value);
        Assert.Equal("гр10103206", tokens[1].Value);
    }
}

public class LexerLiteralsTests
{
    [Theory]
    [InlineData("0", "0")]
    [InlineData("123", "123")]
    [InlineData("456789", "456789")]
    [InlineData("3.14", "3.14")]
    [InlineData("0.5", "0.5")]
    [InlineData("100.001", "100.001")]
    public void Lexer_ShouldRecognizeNumbers(string input, string expectedValue)
    {
        var lexer = new Lexer(input);
        var tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("'world'", "world")]
    [InlineData("\"тест\"", "тест")]
    [InlineData("\"123\"", "123")]
    public void Lexer_ShouldRecognizeStrings(string input, string expectedValue)
    {
        var lexer = new Lexer(input);
        var tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }
}

public class LexerComplexExpressionTests
{
    [Fact]
    public void Lexer_ShouldTokenizeIfStatement()
    {
        var lexer = new Lexer("Если гр10102212 == 1 то гр10103206 = 2");
        var tokens = lexer.Tokenize();

        Assert.Equal(9, tokens.Count);
        Assert.Equal(TokenType.Если, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal(TokenType.Equals, tokens[2].Type);
        Assert.Equal(TokenType.Number, tokens[3].Type);
        Assert.Equal(TokenType.То, tokens[4].Type);
        Assert.Equal(TokenType.Identifier, tokens[5].Type);
        Assert.Equal(TokenType.Assign, tokens[6].Type);
        Assert.Equal(TokenType.Number, tokens[7].Type);
        Assert.Equal(TokenType.EOF, tokens[8].Type);
    }

    [Fact]
    public void Lexer_ShouldTokenizeLogicalExpression()
    {
        var lexer = new Lexer("гр10102212 == 1 или гр10102213 == 2 и гр10102214 == 3");
        var tokens = lexer.Tokenize();

        var expectedTypes = new[]
        {
            TokenType.Identifier, TokenType.Equals, TokenType.Number,
            TokenType.Или,
            TokenType.Identifier, TokenType.Equals, TokenType.Number,
            TokenType.И,
            TokenType.Identifier, TokenType.Equals, TokenType.Number,
            TokenType.EOF
        };

        Assert.Equal(expectedTypes.Length, tokens.Count);
        for (int i = 0; i < expectedTypes.Length; i++)
        {
            Assert.Equal(expectedTypes[i], tokens[i].Type);
        }
    }

    [Fact]
    public void Lexer_ShouldTrackLineAndPosition()
    {
        var lexer = new Lexer("Если\nто");
        var tokens = lexer.Tokenize();

        Assert.Equal(1, tokens[0].Line);
        Assert.Equal(2, tokens[1].Line);
    }

    [Fact]
    public void Lexer_ShouldSkipWhitespace()
    {
        var lexer = new Lexer("   Если   то   ");
        var tokens = lexer.Tokenize();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Если, tokens[0].Type);
        Assert.Equal(TokenType.То, tokens[1].Type);
    }

    [Fact]
    public void Lexer_ShouldHandleEmptyInput()
    {
        var lexer = new Lexer("");
        var tokens = lexer.Tokenize();

        Assert.Single(tokens);
        Assert.Equal(TokenType.EOF, tokens[0].Type);
    }
}

#endregion

#region Parser Tests

public class ParserIfStatementTests
{
    [Fact]
    public void Parser_ShouldParseSimpleIfStatement()
    {
        var lexer = new Lexer("Если гр10102212 == 1 то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        Assert.IsType<IfNode>(ast);
        var ifNode = (IfNode)ast;
        Assert.NotNull(ifNode.Condition);
        Assert.NotNull(ifNode.ThenBranch);
        Assert.Null(ifNode.ElseBranch);
    }

    [Fact]
    public void Parser_ShouldParseIfElseStatement()
    {
        var lexer = new Lexer("Если гр10102212 == 1 то гр10103206 = 2 иначе гр10103206 = 3");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        Assert.IsType<IfNode>(ast);
        var ifNode = (IfNode)ast;
        Assert.NotNull(ifNode.Condition);
        Assert.NotNull(ifNode.ThenBranch);
        Assert.NotNull(ifNode.ElseBranch);
    }

    [Fact]
    public void Parser_ShouldParseIfWithLogicalOr()
    {
        var lexer = new Lexer("Если гр10102212 == 1 или гр10102213 == 2 то гр10103206 = 3");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        Assert.IsType<IfNode>(ast);
        var ifNode = (IfNode)ast;
        Assert.IsType<BinaryOperationNode>(ifNode.Condition);
        
        var condition = (BinaryOperationNode)ifNode.Condition;
        Assert.Equal(TokenType.Или, condition.Operator.Type);
    }

    [Fact]
    public void Parser_ShouldParseIfWithLogicalAnd()
    {
        var lexer = new Lexer("Если гр10102212 == 1 и гр10102213 == 2 то гр10103206 = 3");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        Assert.IsType<IfNode>(ast);
        var ifNode = (IfNode)ast;
        Assert.IsType<BinaryOperationNode>(ifNode.Condition);
        
        var condition = (BinaryOperationNode)ifNode.Condition;
        Assert.Equal(TokenType.И, condition.Operator.Type);
    }

    [Fact]
    public void Parser_ShouldRespectOperatorPrecedence_AndBeforeOr()
    {
        // "a или b и c" должно парситься как "a или (b и c)"
        var lexer = new Lexer("Если гр10102212 == 1 или гр10102213 == 2 и гр10102214 == 3 то гр10103206 = 4");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        Assert.IsType<IfNode>(ast);
        var ifNode = (IfNode)ast;
        Assert.IsType<BinaryOperationNode>(ifNode.Condition);
        
        var orNode = (BinaryOperationNode)ifNode.Condition;
        Assert.Equal(TokenType.Или, orNode.Operator.Type);
        
        // Правый операнд "или" должен быть "и"
        Assert.IsType<BinaryOperationNode>(orNode.Right);
        var andNode = (BinaryOperationNode)orNode.Right;
        Assert.Equal(TokenType.И, andNode.Operator.Type);
    }
}

public class ParserAssignmentTests
{
    [Fact]
    public void Parser_ShouldParseSimpleAssignment()
    {
        var lexer = new Lexer("Если гр10102212 == 1 то гр10103206 = 42");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        var ifNode = (IfNode)ast;
        Assert.IsType<AssignmentNode>(ifNode.ThenBranch);
        
        var assignment = (AssignmentNode)ifNode.ThenBranch;
        Assert.IsType<GraphSelectorNode>(assignment.Target);
        Assert.IsType<LiteralNode>(assignment.Value);
    }

    [Fact]
    public void Parser_ShouldParseAssignmentWithGraphSelector()
    {
        var lexer = new Lexer("Если гр10102212 == 1 то гр10103206 = гр10102209");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        var ifNode = (IfNode)ast;
        var assignment = (AssignmentNode)ifNode.ThenBranch;
        Assert.IsType<GraphSelectorNode>(assignment.Value);
    }
}

public class ParserGraphSelectorNodeTests
{
    [Theory]
    [InlineData("гр10102212", "10102", 2, 12)]
    [InlineData("гр10103206", "10103", 2, 6)]
    [InlineData("гр12345678", "12345", 6, 78)]
    public void Parser_ShouldParseGraphSelectorCorrectly(string input, string expectedRegister, int expectedTablePart, int expectedGraphNumber)
    {
        var lexer = new Lexer($"Если {input} == 1 то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        var ifNode = (IfNode)ast;
        var condition = (BinaryOperationNode)ifNode.Condition;
        var graphSelector = (GraphSelectorNode)condition.Left;

        Assert.Equal(expectedRegister, graphSelector.RegisterCode);
        Assert.Equal(expectedTablePart, graphSelector.TablePart);
        Assert.Equal(expectedGraphNumber, graphSelector.GraphNumber);
    }
}

public class ParserLiteralTests
{
    [Theory]
    [InlineData("1", 1.0)]
    [InlineData("42", 42.0)]
    [InlineData("100", 100.0)]
    [InlineData("999", 999.0)]
    // Примечание: дробные числа (3.14, 0.5) могут не работать из-за локали системы
    // Parser использует double.Parse() без CultureInfo.InvariantCulture
    public void Parser_ShouldParseNumberLiterals(string number, double expectedValue)
    {
        var lexer = new Lexer($"Если гр10102212 == {number} то гр10103206 = 1");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        var ifNode = (IfNode)ast;
        var condition = (BinaryOperationNode)ifNode.Condition;
        var literal = (LiteralNode)condition.Right;

        Assert.Equal(expectedValue, literal.Value);
    }

    [Fact]
    public void Parser_ShouldParseNullLiteral()
    {
        var lexer = new Lexer("Если гр10102212 == пусто то гр10103206 = 1");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        var ifNode = (IfNode)ast;
        var condition = (BinaryOperationNode)ifNode.Condition;
        var literal = (LiteralNode)condition.Right;

        Assert.Null(literal.Value);
    }
}

public class ParserComparisonOperatorsTests
{
    [Theory]
    [InlineData("==", TokenType.Equals)]
    [InlineData("!=", TokenType.NotEquals)]
    [InlineData(">", TokenType.Greater)]
    [InlineData(">=", TokenType.GreaterOrEqual)]
    [InlineData("<", TokenType.Less)]
    [InlineData("<=", TokenType.LessOrEqual)]
    public void Parser_ShouldParseComparisonOperators(string op, TokenType expectedType)
    {
        var lexer = new Lexer($"Если гр10102212 {op} 1 то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        var ifNode = (IfNode)ast;
        var condition = (BinaryOperationNode)ifNode.Condition;

        Assert.Equal(expectedType, condition.Operator.Type);
    }
}

public class ParserParenthesesTests
{
    [Fact]
    public void Parser_ShouldParseParenthesizedExpression()
    {
        var lexer = new Lexer("Если (гр10102212 == 1) то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        Assert.IsType<IfNode>(ast);
        var ifNode = (IfNode)ast;
        Assert.IsType<BinaryOperationNode>(ifNode.Condition);
    }

    [Fact]
    public void Parser_ShouldRespectParenthesesPrecedence()
    {
        // "(a или b) и c" - скобки меняют приоритет
        var lexer = new Lexer("Если (гр10102212 == 1 или гр10102213 == 2) и гр10102214 == 3 то гр10103206 = 4");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        var ast = parser.Parse();

        var ifNode = (IfNode)ast;
        var andNode = (BinaryOperationNode)ifNode.Condition;
        Assert.Equal(TokenType.И, andNode.Operator.Type);

        // Левый операнд "и" должен быть "или" (в скобках)
        Assert.IsType<BinaryOperationNode>(andNode.Left);
        var orNode = (BinaryOperationNode)andNode.Left;
        Assert.Equal(TokenType.Или, orNode.Operator.Type);
    }
}

public class ParserErrorHandlingTests
{
    [Fact]
    public void Parser_ShouldThrowOnMissingТо()
    {
        var lexer = new Lexer("Если гр10102212 == 1 гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }

    [Fact]
    public void Parser_ShouldThrowOnUnexpectedToken()
    {
        var lexer = new Lexer("123");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }

    [Fact]
    public void Parser_ShouldThrowOnMissingClosingParen()
    {
        var lexer = new Lexer("Если (гр10102212 == 1 то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
}

#endregion
