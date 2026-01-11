
namespace ALCompiler.Lexing.Enum
{
    public enum TokenType
    {
        // Ключевые слова
        Если, То, Иначе, И, Или, Не, Пусто,

        // Операторы
        Equals, NotEquals, Greater, GreaterOrEqual, Less, LessOrEqual,
        Assign, Plus, Minus, Multiply, Divide, Mod,

        // Селекторы данных
        Графа, Регистр, ВсеГрафы, ВсеЗаписи,

        // Литералы
        Number, String, Boolean, Date,

        // Символы
        LParen, RParen, Comma, Dot, Colon,

        // Специальные
        EOF, Invalid, Identifier
    }
}
