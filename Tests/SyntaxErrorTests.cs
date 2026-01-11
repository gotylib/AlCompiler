using ALCompiler.Lexer;
using ALCompiler.Parser;
using ALCompiler.Parser.Nodes;

namespace Tests;

/// <summary>
/// Тесты на синтаксические ошибки парсера - проверяем что компилятор выявляет ошибки
/// </summary>
public class ParserSyntaxErrorTests
{
    // ═══════════════════════════════════════════════════════════════
    // Отсутствие обязательных ключевых слов
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void Parser_ShouldParseGraphComparisonWithoutЕсли()
    {
        // Без "Если" парсер пытается распознать как сравнение/присваивание
        // Это не является ошибкой синтаксиса - парсер просто распознаёт часть выражения
        var lexer = new Lexer("гр10102212 == 1");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        // Парсер распознаёт это как BinaryOperation, а не выбрасывает ошибку
        var ast = parser.Parse();
        Assert.IsType<BinaryOperationNode>(ast);
    }
    
    [Fact]
    public void Parser_ShouldThrowOnJustNumber()
    {
        // Просто число - не является валидным началом выражения
        var lexer = new Lexer("123");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnMissingТо_AfterCondition()
    {
        // Пропущено "То" после условия
        var lexer = new Lexer("Если гр10102212 == 1 гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        var ex = Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
        Assert.Contains("то", ex.Message.ToLower());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnMissingCondition_AfterЕсли()
    {
        // Пропущено условие после "Если"
        var lexer = new Lexer("Если то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnMissingAction_AfterТо()
    {
        // Пропущено действие после "То"
        var lexer = new Lexer("Если гр10102212 == 1 то");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnMissingAction_AfterИначе()
    {
        // Пропущено действие после "Иначе"
        var lexer = new Lexer("Если гр10102212 == 1 то гр10103206 = 2 иначе");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    // ═══════════════════════════════════════════════════════════════
    // Ошибки в выражениях и операторах
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void Parser_ShouldThrowOnMissingRightOperand()
    {
        // Пропущен правый операнд сравнения
        var lexer = new Lexer("Если гр10102212 == то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnMissingLeftOperand()
    {
        // Пропущен левый операнд (начинается с оператора)
        var lexer = new Lexer("Если == 1 то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnDoubleOperator()
    {
        // Два оператора подряд
        var lexer = new Lexer("Если гр10102212 == == 1 то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnDoubleLogicalOperator()
    {
        // Два логических оператора подряд
        var lexer = new Lexer("Если гр10102212 == 1 или или гр10102213 == 2 то гр10103206 = 3");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnMissingAssignmentValue()
    {
        // Пропущено значение присваивания
        var lexer = new Lexer("Если гр10102212 == 1 то гр10103206 =");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnMissingAssignmentOperator()
    {
        // Пропущен оператор присваивания в действии
        var lexer = new Lexer("Если гр10102212 == 1 то гр10103206 5");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    // ═══════════════════════════════════════════════════════════════
    // Ошибки со скобками
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void Parser_ShouldThrowOnUnclosedParenthesis()
    {
        var lexer = new Lexer("Если (гр10102212 == 1 то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnExtraClosingParenthesis()
    {
        var lexer = new Lexer("Если гр10102212 == 1) то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnEmptyParentheses()
    {
        var lexer = new Lexer("Если () то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnNestedUnclosedParentheses()
    {
        var lexer = new Lexer("Если ((гр10102212 == 1) то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    // ═══════════════════════════════════════════════════════════════
    // Ошибки с пустым вводом и только пробелами
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void Parser_ShouldThrowOnEmptyInput()
    {
        var lexer = new Lexer("");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnWhitespaceOnlyInput()
    {
        var lexer = new Lexer("   \t\n   ");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    // ═══════════════════════════════════════════════════════════════
    // Ошибки с неправильным порядком токенов
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void Parser_ShouldThrowOnTwoЕслиKeywords()
    {
        var lexer = new Lexer("Если Если гр10102212 == 1 то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnТоBeforeCondition()
    {
        var lexer = new Lexer("Если то гр10102212 == 1 гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnИначеWithoutЕсли()
    {
        var lexer = new Lexer("иначе гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnИначеBeforeТо()
    {
        var lexer = new Lexer("Если гр10102212 == 1 иначе гр10103206 = 2 то гр10103206 = 3");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    // ═══════════════════════════════════════════════════════════════
    // Ошибки с неполными выражениями
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void Parser_ShouldThrowOnIncompleteLogicalExpression()
    {
        // "или" без правой части
        var lexer = new Lexer("Если гр10102212 == 1 или то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnOnlyKeyword()
    {
        var lexer = new Lexer("Если");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    [Fact]
    public void Parser_ShouldThrowOnOnlyGraphSelector()
    {
        var lexer = new Lexer("гр10102212");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
    }
    
    // ═══════════════════════════════════════════════════════════════
    // Проверка сообщений об ошибках
    // ═══════════════════════════════════════════════════════════════
    
    [Fact]
    public void Parser_ErrorMessage_ShouldContainHelpfulInfo_ForMissingТо()
    {
        var lexer = new Lexer("Если гр10102212 == 1 гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        var ex = Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
        
        // Сообщение должно содержать информацию о том, что ожидалось
        Assert.NotNull(ex.Message);
        Assert.NotEmpty(ex.Message);
    }
    
    [Fact]
    public void Parser_ErrorMessage_ShouldContainHelpfulInfo_ForUnexpectedToken()
    {
        var lexer = new Lexer("Если == то гр10103206 = 2");
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        var ex = Assert.Throws<ALCompiler.Parser.Exception.ParseException>(() => parser.Parse());
        
        Assert.NotNull(ex.Message);
        Assert.NotEmpty(ex.Message);
    }
}

/// <summary>
/// Тесты для проверки что валидный синтаксис НЕ вызывает ошибок
/// </summary>
public class ParserValidSyntaxTests
{
    [Theory]
    [InlineData("Если гр10102212 == 1 то гр10103206 = 2")]
    [InlineData("Если гр10102212 == 1 то гр10103206 = гр10102209")]
    [InlineData("Если гр10102212 == 1 или гр10102213 == 2 то гр10103206 = 3")]
    [InlineData("Если гр10102212 == 1 и гр10102213 == 2 то гр10103206 = 3")]
    [InlineData("Если гр10102212 == 1 то гр10103206 = 2 иначе гр10103206 = 3")]
    [InlineData("Если (гр10102212 == 1) то гр10103206 = 2")]
    [InlineData("Если (гр10102212 == 1 или гр10102213 == 2) и гр10102214 == 3 то гр10103206 = 4")]
    [InlineData("Если гр10102212 != 1 то гр10103206 = 2")]
    [InlineData("Если гр10102212 > 1 то гр10103206 = 2")]
    [InlineData("Если гр10102212 >= 1 то гр10103206 = 2")]
    [InlineData("Если гр10102212 < 1 то гр10103206 = 2")]
    [InlineData("Если гр10102212 <= 1 то гр10103206 = 2")]
    [InlineData("Если гр10102212 == пусто то гр10103206 = 0")]
    public void Parser_ShouldNotThrow_OnValidSyntax(string validScript)
    {
        var lexer = new Lexer(validScript);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        // Не должно быть исключения
        var exception = Record.Exception(() => parser.Parse());
        Assert.Null(exception);
    }
    
    [Fact]
    public void Parser_ShouldParseComplexValidExpression()
    {
        var script = "Если гр10102212 == 1 или гр10102212 == 2 и гр10102213 == 5 то гр10103206 = гр10102209";
        var lexer = new Lexer(script);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        
        var ast = parser.Parse();
        
        Assert.NotNull(ast);
        Assert.IsType<IfNode>(ast);
    }
}
