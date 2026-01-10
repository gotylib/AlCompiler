using ALCompiler.Lexer;
using ALCompiler.Parser;
using ALCompiler.Visualizer;

var lexer = new Lexer("Если гр10102212 == 1 или гр10102212 == 2 то гр10103206 = гр10102209");
var tokens = lexer.Tokenize();

// Визуализация токенов
Visualizer.PrintTokens(tokens);

var parser = new Parser(tokens);
var ast = parser.Parse();

// Визуализация AST
Visualizer.PrintAST(ast);
