using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Canto34.Tests.Parsing
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void ShouldLexTokens()
        {
            var testLexer = new TestLexer("foo bar baz - quux $corge");
            var tokens = testLexer.Tokens().ToList();
            var expected = @"foo type=foo:1, l.1 ch.1
bar type=bar:2, l.1 ch.5
baz type=baz:3, l.1 ch.9
- type=-:6, l.1 ch.13
quux type=quux:4, l.1 ch.15
$corge type=id:5, l.1 ch.20";
            var actual = string.Join("\r\n", tokens.ConvertAll(t => string.Format("{0} type={1}:{2}, l.{3} ch.{4}", t.Content, testLexer.NameOfTokenType(t.TokenType), t.TokenType, t.Line, t.Character)));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ShouldBeAbleToRetrieveTokenNames()
        {
            var testLexer = new TestLexer("foo bar baz - quux");
            Assert.AreEqual("foo", testLexer.NameOfTokenType(1));
            Assert.AreEqual("id", testLexer.NameOfTokenType(5));
            Assert.AreEqual("whitespace", testLexer.NameOfTokenType(-1));
        }
    }

    public class TestLexer: LexerBase
    {
        public TestLexer(string input): this(new LexerInput(input))
        {
        }

        public TestLexer(LexerInput input): base(input)
        {
            this.AddPattern(-1, @"\s+", "whitespace");
            this.AddKeyword(1, "foo");
            this.AddKeyword(2, "bar");
            this.AddKeyword(3,"baz");
            this.AddKeyword(4, "quux");
            this.AddPattern(5, @"\$[a-z]+", "id");
            this.AddLiteral(6, "-");
        }
    }
}
