
using ALCompiler.Lexer.Enum;

namespace ALCompiler.Lexer
{
    public class Lexer
    {
        private readonly string _source;
        private int _position;
        private int _line;

        private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        {"если", TokenType.Если},
        {"то", TokenType.То},
        {"иначе", TokenType.Иначе},
        {"и", TokenType.И},
        {"или", TokenType.Или},
        {"не", TokenType.Не},
        {"пусто", TokenType.Пусто},
        {"графа", TokenType.Графа},
        {"регистр", TokenType.Регистр},
        {"всеграфы", TokenType.ВсеГрафы},
        {"всезаписи", TokenType.ВсеЗаписи}
    };

        public Lexer(string source)
        {
            _source = source;
            _position = 0;
            _line = 1;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _source.Length)
            {
                var current = _source[_position];

                // Пропускаем пробелы
                if (char.IsWhiteSpace(current))
                {
                    if (current == '\n') _line++;
                    _position++;
                    continue;
                }

                // Графа (гр10102212)
                if (current == 'г' && (_position + 1 < _source.Length && _source[_position + 1] == 'р'))
                {
                    tokens.Add(ReadGraphSelector());
                    continue;
                }

                // Числа
                if (char.IsDigit(current))
                {
                    tokens.Add(ReadNumber());
                    continue;
                }

                // Идентификаторы и ключевые слова
                if (char.IsLetter(current))
                {
                    tokens.Add(ReadIdentifier());
                    continue;
                }

                // Строки
                if (current == '"' || current == '\'')
                {
                    tokens.Add(ReadString(current));
                    continue;
                }

                // Операторы
                switch (current)
                {
                    case '=':
                        if (Peek() == '=')
                        {
                            _position++;
                            tokens.Add(new Token(TokenType.Equals, "==", _line, _position));
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.Assign, "=", _line, _position));
                        }
                        break;

                    case '!':
                        if (Peek() == '=')
                        {
                            _position++;
                            tokens.Add(new Token(TokenType.NotEquals, "!=", _line, _position));
                        }
                        break;

                    case '>':
                        if (Peek() == '=')
                        {
                            _position++;
                            tokens.Add(new Token(TokenType.GreaterOrEqual, ">=", _line, _position));
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.Greater, ">", _line, _position));
                        }
                        break;

                    case '<':
                        if (Peek() == '=')
                        {
                            _position++;
                            tokens.Add(new Token(TokenType.LessOrEqual, "<=", _line, _position));
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.Less, "<", _line, _position));
                        }
                        break;

                    case '+':
                        tokens.Add(new Token(TokenType.Plus, "+", _line, _position));
                        break;

                    case '-':
                        tokens.Add(new Token(TokenType.Minus, "-", _line, _position));
                        break;

                    case '*':
                        tokens.Add(new Token(TokenType.Multiply, "*", _line, _position));
                        break;

                    case '/':
                        tokens.Add(new Token(TokenType.Divide, "/", _line, _position));
                        break;

                    case '%':
                        tokens.Add(new Token(TokenType.Mod, "%", _line, _position));
                        break;

                    case '(':
                        tokens.Add(new Token(TokenType.LParen, "(", _line, _position));
                        break;

                    case ')':
                        tokens.Add(new Token(TokenType.RParen, ")", _line, _position));
                        break;

                    case ',':
                        tokens.Add(new Token(TokenType.Comma, ",", _line, _position));
                        break;

                    case '.':
                        tokens.Add(new Token(TokenType.Dot, ".", _line, _position));
                        break;

                    case ':':
                        tokens.Add(new Token(TokenType.Colon, ":", _line, _position));
                        break;

                    default:
                        tokens.Add(new Token(TokenType.Invalid, current.ToString(), _line, _position));
                        break;
                }

                _position++;
            }

            tokens.Add(new Token(TokenType.EOF, "", _line, _position));
            return tokens;
        }

        private Token ReadGraphSelector()
        {
            var start = _position;
            _position += 2; // Пропускаем "гр"

            // Читаем номер графы (8 или более цифр)
            while (_position < _source.Length && char.IsDigit(_source[_position]))
            {
                _position++;
            }

            var value = _source.Substring(start, _position - start);
            return new Token(TokenType.Identifier, value, _line, _position);
        }

        private Token ReadNumber()
        {
            var start = _position;
            var hasDot = false;

            while (_position < _source.Length &&
                   (char.IsDigit(_source[_position]) || _source[_position] == '.'))
            {
                if (_source[_position] == '.')
                {
                    if (hasDot) break; // Уже была точка
                    hasDot = true;
                }
                _position++;
            }

            var value = _source.Substring(start, _position - start);
            return new Token(TokenType.Number, value, _line, _position);
        }

        private Token ReadIdentifier()
        {
            var start = _position;
            while (_position < _source.Length &&
                   (char.IsLetterOrDigit(_source[_position]) || _source[_position] == '_'))
            {
                _position++;
            }

            var value = _source.Substring(start, _position - start);

            if (Keywords.TryGetValue(value.ToLower(), out var keywordType))
            {
                return new Token(keywordType, value, _line, _position);
            }

            return new Token(TokenType.Identifier, value, _line, _position);
        }

        private Token ReadString(char quote)
        {
            _position++; // Пропускаем открывающую кавычку
            var start = _position;

            while (_position < _source.Length && _source[_position] != quote)
            {
                _position++;
            }

            var value = _source.Substring(start, _position - start);

            if (_position < _source.Length && _source[_position] == quote)
            {
                _position++; // Пропускаем закрывающую кавычку
            }

            return new Token(TokenType.String, value, _line, _position);
        }

        private char Peek() =>
            _position + 1 < _source.Length ? _source[_position + 1] : '\0';
    }
}
