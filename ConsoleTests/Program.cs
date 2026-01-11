using ALCompiler.CodeGenerator;
using ALCompiler.CodeGenerator.RegisterModel;
using ALCompiler.Lexer;
using ALCompiler.Parser;
using ALCompiler.Visualizer;

Dictionary<string, TaxRegister> _registers = [];

var lexer = new Lexer("Если гр10102212 == 1 или гр10102212 == 2 и гр10102212 == 5 то гр10103206 = гр10102209");
var tokens = lexer.Tokenize();

// Визуализация токенов
Visualizer.PrintTokens(tokens);

var parser = new Parser(tokens);
var ast = parser.Parse();

// Визуализация AST
Visualizer.PrintAST(ast);

var generator = new TaxRegisterCodeGenerator(_registers);
var csharpCode = generator.GenerateCode(ast);
Console.WriteLine(csharpCode);
