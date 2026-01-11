namespace ALCompiler.Parsing.Nodes;

public class LiteralNode(object? value) : ASTNode
{
    public object? Value { get; } = value;
}