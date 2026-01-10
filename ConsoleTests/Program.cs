using ALCompiler.Lexer;

var lexer = new Lexer("Если гр10102212 = 1 или гр10102212 = 2 и гр10102213 = 2, то гр10103206 = гр10102209");
var tokens = lexer.Tokenize();

foreach(var token in tokens)
{
    Console.Write($"{token.Value} ");
    Console.Write($"{token.Type} ");
    Console.Write($"{token.Line} ");
    Console.Write($"{token.Position} ");
    Console.WriteLine("\n");
}