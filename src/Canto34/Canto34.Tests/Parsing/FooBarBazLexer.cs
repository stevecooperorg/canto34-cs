using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canto34.Tests.Parsing
{
    public class FooBarBazLexer : LexerBase
    {
        public FooBarBazLexer(string input)
            : this(new LexerInput(input))
        {
        }

        public FooBarBazLexer(LexerInput input)
            : base(input)
        {
            this.AddPattern(-1, @"\s+", "whitespace");
            this.AddKeyword(1, "foo");
            this.AddKeyword(2, "bar");
            this.AddKeyword(3, "baz");
            this.AddKeyword(4, "quux");
            this.AddPattern(5, @"\$[a-z]+", "id");
            this.AddLiteral(6, "-");
        }
    }
}
