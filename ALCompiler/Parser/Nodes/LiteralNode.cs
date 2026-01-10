using ALCompiler.Lexer;

namespace ALCompiler.Parser.Nodes;

public class LiteralNode(object? value) : ASTNode
{
    public object? Value { get; } = value;
}