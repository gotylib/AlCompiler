namespace ALCompiler.Parsing.Nodes;

public class GraphSelectorNode : ASTNode
{
    public string RegisterCode { get; }  // 10101
    public int TablePart { get; }        // 2 (табличная часть)
    public int GraphNumber { get; }      // 12
    
    public GraphSelectorNode(string? fullCode)
    {
        // гр10102212 → 10101.2.212
        if (!fullCode.StartsWith("гр", StringComparison.OrdinalIgnoreCase)) return;
        
        var numbers = fullCode[2..];
            
        if (numbers.Length < 8) return;
            
        RegisterCode = numbers[..5];
        TablePart = int.Parse(numbers.Substring(5, 1));
        GraphNumber = int.Parse(numbers[6..]);
    }
}