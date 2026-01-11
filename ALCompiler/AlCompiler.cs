using ALCompiler.CodeGenerator;
using ALCompiler.Lexing;
using ALCompiler.Parsing;
using ALCompiler.Visualization;

namespace ALCompilation;

public static class AlCompiler
{
    public static string Compile(string code)
    {
        var lexer = new Lexer(code);
        var tokens = lexer.Tokenize();
        
        var parser = new Parser(tokens);
        var ast = parser.Parse();
        
        var generator = new TaxRegisterCodeGenerator();
        var generatedCode = generator.GenerateCode(ast);
        
        return generatedCode;
    }

    public static string CompileWithVisualization(string code)
    {
        var lexer = new Lexer(code);
        var tokens = lexer.Tokenize();
        Visualizer.PrintTokens(tokens);
        
        var parser = new Parser(tokens);
        var ast = parser.Parse();
        Visualizer.PrintAST(ast);
        
        var generator = new TaxRegisterCodeGenerator();
        var generatedCode = generator.GenerateCode(ast);
        
        return generatedCode;
        
    }
}