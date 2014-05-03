using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canto34.Tests.Parsing
{
    public class TestLexer2 : LexerBase
    {
        public TestLexer2(string input)
            : base(input)
        {
            this.AddKeyword(1, "foo");
            this.AddKeyword(2, "bar");
            this.AddKeyword(3, "baz");
            this.AddPattern(-1, "\\s+", "whitespace");
        }
    }
}
