using ALCompiler.Lexer;
using ALCompiler.Lexer.Enum;

namespace Tests
{
    public class UnitTests
    {
        [Fact]
        public void Test_Lexer()
        {
            // Arrange
            var lexer = new Lexer("Если гр10102212 = 1 или гр10102212 = 2 и гр10102213 = 2, то гр10103206 = гр10102209");

            //Act
            var tokens = lexer.Tokenize();

            // Assert
            Assert.NotEmpty(tokens);

            foreach (var token in tokens.Where(token => token.Type != TokenType.EOF))
            {
                Assert.NotEmpty(token.Value);
            }
        }
    }
}