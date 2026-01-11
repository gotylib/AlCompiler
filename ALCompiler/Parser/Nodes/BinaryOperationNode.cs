using ALCompiler.Lexing;

namespace ALCompiler.Parsing.Nodes;

public class BinaryOperationNode : ASTNode
{
    public ASTNode Left { get; }
    public Token Operator { get; }
    public ASTNode Right { get; }
    
    public BinaryOperationNode(ASTNode left, Token op, ASTNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}