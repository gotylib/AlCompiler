using ALCompiler.Lexer;
using ALCompiler.Lexer.Enum;

namespace Tests;

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

/// <summary>
/// Тесты на невалидный ввод для лексера
/// </summary>
public class LexerInvalidInputTests
{
    [Fact]
    public void Lexer_ShouldMarkInvalidCharactersAsInvalid()
    {
        // Символ @ не является валидным
        var lexer = new Lexer("@");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Invalid, tokens[0].Type);
    }
    
    [Theory]
    [InlineData("$")]
    [InlineData("#")]
    [InlineData("~")]
    [InlineData("`")]
    [InlineData("\\")]
    public void Lexer_ShouldMarkSpecialCharsAsInvalid(string invalidChar)
    {
        var lexer = new Lexer(invalidChar);
        var tokens = lexer.Tokenize();
        
        Assert.Contains(tokens, t => t.Type == TokenType.Invalid);
    }
    
    [Fact]
    public void Lexer_ShouldHandleUnclosedString()
    {
        // Незакрытая строка
        var lexer = new Lexer("\"незакрытая строка");
        var tokens = lexer.Tokenize();
        
        // Лексер должен вернуть токен (строка читается до конца)
        Assert.NotEmpty(tokens);
        // Проверяем что строка была прочитана (пусть и без закрывающей кавычки)
        Assert.Equal(TokenType.String, tokens[0].Type);
    }
    
    [Fact]
    public void Lexer_ShouldHandleEmptyString()
    {
        var lexer = new Lexer("\"\"");
        var tokens = lexer.Tokenize();
        
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal("", tokens[0].Value);
    }
}
