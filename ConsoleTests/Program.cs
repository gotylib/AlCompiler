using ALCompiler.CodeGenerator;
using ALCompiler.CodeGenerator.Analyzer;
using ALCompiler.CodeGenerator.RegisterModel;
using ALCompiler.Lexer;
using ALCompiler.Parser;
using ALCompiler.Parser.Nodes;
using ALCompiler.Visualizer;

Dictionary<string, TaxRegister> _registers = [];

// Тестовое выражение
var expression = "Если гр10102212 == 1 или гр10102212 == 2 и гр10102213 == 5 то гр10103206 = гр10102209";

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                    БЛОЧНАЯ КОДОГЕНЕРАЦИЯ                     ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"Выражение: {expression}");
Console.WriteLine();

// Лексер
var lexer = new Lexer(expression);
var tokens = lexer.Tokenize();

Console.WriteLine("═══════════════════ ТОКЕНЫ ═══════════════════");
Visualizer.PrintTokens(tokens);

// Парсер
var parser = new Parser(tokens);
var ast = parser.Parse();

Console.WriteLine("\n═══════════════════ AST ═══════════════════");
Visualizer.PrintAST(ast);

// Анализ AST
if (ast is IfNode ifNode)
{
    Console.WriteLine("\n═══════════════════ АНАЛИЗ (СБОР ДАННЫХ) ═══════════════════");
    
    var analyzer = new IfBlockAnalyzer();
    var blockInfo = analyzer.Analyze(ifNode);
    
    Console.WriteLine("\nСобранные данные:");
    Console.WriteLine($"  Условий: {blockInfo.Conditions.Count}");
    
    Console.WriteLine("\n  Условия:");
    foreach (var cond in blockInfo.Conditions)
    {
        var logOp = cond.LogicalOperator != null ? $"[{cond.LogicalOperator}] " : "[первое] ";
        Console.WriteLine($"    {logOp}гр{cond.RegisterCode}_{cond.GraphNumber} {cond.Operator} {cond.Value}");
    }
    
    Console.WriteLine("\n  Then-блок:");
    if (blockInfo.ThenAssignment != null)
    {
        var assign = blockInfo.ThenAssignment;
        var value = assign.IsLiteralAssignment 
            ? assign.LiteralValue?.ToString() 
            : $"гр{assign.SourceRegister}_{assign.SourceGraph}";
        Console.WriteLine($"    гр{assign.TargetRegister}_{assign.TargetGraph} = {value}");
    }
    
    if (blockInfo.ElseAssignment != null)
    {
        Console.WriteLine("\n  Else-блок:");
        var assign = blockInfo.ElseAssignment;
        var value = assign.IsLiteralAssignment 
            ? assign.LiteralValue?.ToString() 
            : $"гр{assign.SourceRegister}_{assign.SourceGraph}";
        Console.WriteLine($"    гр{assign.TargetRegister}_{assign.TargetGraph} = {value}");
    }
    
    Console.WriteLine("\n  Участвующие регистры в условиях:");
    foreach (var reg in blockInfo.GetConditionRegisters())
    {
        var graphs = string.Join(", ", blockInfo.GetConditionGraphs(reg));
        Console.WriteLine($"    {reg}: графы [{graphs}]");
    }
}

// Генерация кода (НОВЫЙ БЛОЧНЫЙ ПОДХОД)
Console.WriteLine("\n═══════════════════ СГЕНЕРИРОВАННЫЙ КОД ═══════════════════");
var blockGenerator = new TaxRegisterCodeGenerator(_registers);
var blockCode = blockGenerator.GenerateCode(ast);
Console.WriteLine(blockCode);

