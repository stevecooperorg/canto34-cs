using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canto34.Tests.Parsing
{
    public class CalcLexer : LexerBase
    {
        public static readonly int PLUS = NextTokenType();
        public static readonly int SEMI = NextTokenType();
        public static readonly int ID = NextTokenType();
        public static readonly int NUMBER = NextTokenType();
        public static readonly int EQUALS = NextTokenType();

        public CalcLexer(string input)
            : base(input)
        {
            this.AddLiteral(PLUS, "+");
            this.AddLiteral(SEMI, ";");
            this.AddLiteral(EQUALS, "=");
            this.AddPattern(ID, "[a-z]+", "ID");
            this.AddPattern(NUMBER, "[0-9]+", "NUMBER");
            this.AddLiteral(PLUS, "+");
            this.StandardTokens.AddWhitespace();
        }
    }
}
