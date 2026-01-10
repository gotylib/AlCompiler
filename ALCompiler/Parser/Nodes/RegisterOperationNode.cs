namespace ALCompiler.Parser.Nodes;

public class RegisterOperationNode : ASTNode
{
    public enum OperationType
    {
        ContainsAll,
        ContainsAny,
        Sum,
        Average,
        Max,
        Min,
        Count
    }
    
    public OperationType Operation { get; }
    public GraphSelectorNode Source { get; }
    public ASTNode Condition { get; }
    public int? TargetGraph { get; }
    
    public RegisterOperationNode(OperationType operation, GraphSelectorNode source, 
        ASTNode condition = null, int? targetGraph = null)
    {
        Operation = operation;
        Source = source;
        Condition = condition;
        TargetGraph = targetGraph;
    }
}