namespace ALCompiler.Parser.Nodes;

public class IfNode(ASTNode condition, ASTNode thenBranch, ASTNode elseBranch = null)
    : ASTNode
{
    public ASTNode Condition { get; } = condition;
    public ASTNode ThenBranch { get; } = thenBranch;
    public ASTNode ElseBranch { get; } = elseBranch;
}