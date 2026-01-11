using ALCompiler.Lexing.Enum;

namespace ALCompiler.Lexing
{
    public class Token(TokenType type, string? value, int line, int position)
    {
        public TokenType Type { get; } = type;
        public string? Value { get; } = value;
        public int Line { get; } = line;
        public int Position { get; } = position;
    }
}
