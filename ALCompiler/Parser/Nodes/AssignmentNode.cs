namespace ALCompiler.Parser.Nodes;

public class AssignmentNode(GraphSelectorNode target, ASTNode value) : ASTNode
{
    public GraphSelectorNode Target { get; } = target;
    public ASTNode Value { get; } = value;
}