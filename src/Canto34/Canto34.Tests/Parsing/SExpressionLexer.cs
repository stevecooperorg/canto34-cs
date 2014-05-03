using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canto34.Tests.Parsing
{
    public class SExpressionLexer : LexerBase
    {
        public const int OP = 1;
        public const int CL = 2;
        public const int ATOM = 3;

        public SExpressionLexer(string input)
            : base(input)
        {
            this.AddLiteral(OP, "(");
            this.AddLiteral(CL, ")");
            this.AddPattern(ATOM, "[a-z]+", "atom");
            this.StandardTokens.AddWhitespace();
        }
    }
}
