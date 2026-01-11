using ALCompiler.CodeGenerator.RegisterModel;
using ALCompiler.Lexer;
using ALCompiler.Lexer.Enum;
using ALCompiler.Parser;
using ALCompiler.Parser.Nodes;
using System.Text;

namespace ALCompiler.CodeGenerator
{
    public class TaxRegisterCodeGenerator
    {
        private readonly Dictionary<string, TaxRegister> _registers;

        public TaxRegisterCodeGenerator(Dictionary<string, TaxRegister> registers)
        {
            _registers = registers;
        }

        public string GenerateCode(ASTNode ast)
        {
            var code = new StringBuilder();

            // Начало метода проверки
            code.AppendLine("public bool Validate()");
            code.AppendLine("{");
            code.AppendLine("    var errors = new List<string>();");
            code.AppendLine("    var isValid = true;");
            code.AppendLine();

            GenerateNodeCode(ast, code, 1);

            code.AppendLine();
            code.AppendLine("    return isValid;");
            code.AppendLine("}");

            return code.ToString();
        }

        private void GenerateNodeCode(ASTNode node, StringBuilder code, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 4);

            switch (node)
            {
                case IfNode ifNode:
                    code.Append(indent);
                    code.Append("if (");
                    GenerateExpressionCode(ifNode.Condition, code);
                    code.AppendLine(")");
                    code.Append(indent);
                    code.AppendLine("{");
                    GenerateNodeCode(ifNode.ThenBranch, code, indentLevel + 1);
                    code.Append(indent);
                    code.AppendLine("}");

                    if (ifNode.ElseBranch != null)
                    {
                        code.Append(indent);
                        code.AppendLine("else");
                        code.Append(indent);
                        code.AppendLine("{");
                        GenerateNodeCode(ifNode.ElseBranch, code, indentLevel + 1);
                        code.Append(indent);
                        code.AppendLine("}");
                    }
                    break;

                case AssignmentNode assignment:
                    // Обычное присваивание значения
                    code.Append(indent);
                    code.Append(GenerateGraphAccessor(assignment.Target));
                    code.Append(" = ");
                    GenerateExpressionCode(assignment.Value, code);
                    code.AppendLine(";");
                    break;

                case RegisterOperationNode operation:
                    // Специальные операции над регистрами
                    code.Append(indent);
                    code.AppendLine(GenerateRegisterOperation(operation));
                    break;

                case BinaryOperationNode binaryOp:
                    GenerateExpressionCode(binaryOp, code);
                    break;
            }
        }

        private string GenerateGraphAccessor(GraphSelectorNode graph)
        {
            return $"GetRegisterValue(\"{graph.RegisterCode}\", {graph.TablePart}, {graph.GraphNumber})";
        }

        private string GenerateRegisterOperation(RegisterOperationNode operation)
        {
            // Проверяем, что TargetGraph указан
            if (!operation.TargetGraph.HasValue)
            {
                throw new InvalidOperationException(
                    $"Для операции {operation.Operation} не указана целевая графа");
            }

            var targetGraph = operation.TargetGraph.Value;
            var sourceReg = operation.Source.RegisterCode;
            var sourceGraph = operation.Source.GraphNumber;
            var targetReg = operation.Source.RegisterCode; // Или берем из другого свойства, если есть

            var operationCode = operation.Operation switch
            {
                RegisterOperationNode.OperationType.ContainsAll =>
                    GenerateContainsAllOperation(sourceReg, sourceGraph, targetReg, targetGraph),

                RegisterOperationNode.OperationType.ContainsAny =>
                    GenerateContainsAnyOperation(sourceReg, sourceGraph, targetReg, targetGraph),

                RegisterOperationNode.OperationType.Sum =>
                    GenerateSumOperation(sourceReg, sourceGraph, targetReg, targetGraph),

                _ => throw new NotSupportedException($"Операция {operation.Operation} не поддерживается")
            };

            return operationCode;
        }

        private string GenerateContainsAllOperation(string sourceReg, int sourceGraph,
                                                  string targetReg, int targetGraph)
        {
            return $$"""
                // Проверка: все значения гр{{sourceReg}}2{{sourceGraph:000}} содержатся в гр{{targetReg}}2{{targetGraph:000}}
                var sourceValues = GetRegisterColumn("{{sourceReg}}", 2, {{sourceGraph}});
                var targetValues = GetRegisterColumn("{{targetReg}}", 2, {{targetGraph}});
                isValid &= sourceValues.All(sv => targetValues.Contains(sv));
                if (!isValid) errors.Add("Не все значения содержатся в целевой графе");
                """;
        }

        private string GenerateContainsAnyOperation(string sourceReg, int sourceGraph,
                                                  string targetReg, int targetGraph)
        {
            return $$"""
                // Проверка: хотя бы одно значение гр{{sourceReg}}2{{sourceGraph:000}} содержится в целевой графе
                var sourceValues = GetRegisterColumn("{{sourceReg}}", 2, {{sourceGraph}});
                var targetValues = GetRegisterColumn("{{targetReg}}", 2, {{targetGraph}});
                isValid &= sourceValues.Any(sv => targetValues.Contains(sv));
                """;
        }

        private string GenerateSumOperation(string sourceReg, int sourceGraph,
                                           string targetReg, int targetGraph)
        {
            return $$"""
                // Сумма значений гр{{sourceReg}}2{{sourceGraph:000}}
                var sum = GetRegisterColumn("{{sourceReg}}", 2, {{sourceGraph}})
                            .Select(v => Convert.ToDecimal(v))
                            .Sum();
                SetRegisterValue("{{targetReg}}", 2, {{targetGraph}}, sum);
                """;
        }

        private void GenerateExpressionCode(ASTNode node, StringBuilder code)
        {
            switch (node)
            {
                case GraphSelectorNode graph:
                    code.Append(GenerateGraphAccessor(graph));
                    break;

                case LiteralNode literal:
                    if (literal.Value == null)
                        code.Append("null");
                    else if (literal.Value is string)
                        code.Append($"\"{literal.Value}\"");
                    else
                        code.Append(literal.Value);
                    break;

                case BinaryOperationNode binary:
                    code.Append("(");
                    GenerateExpressionCode(binary.Left, code);
                    code.Append($" {GetOperator(binary.Operator)} ");
                    GenerateExpressionCode(binary.Right, code);
                    code.Append(")");
                    break;
            }
        }

        private string GetOperator(Token token)
        {
            return token.Type switch
            {
                TokenType.Equals => "==",
                TokenType.NotEquals => "!=",
                TokenType.Greater => ">",
                TokenType.GreaterOrEqual => ">=",
                TokenType.Less => "<",
                TokenType.LessOrEqual => "<=",
                TokenType.Или => "||",
                TokenType.И => "&&",
                TokenType.Plus => "+",
                TokenType.Minus => "-",
                TokenType.Multiply => "*",
                TokenType.Divide => "/",
                _ => token.Value
            };
        }
    }
}
