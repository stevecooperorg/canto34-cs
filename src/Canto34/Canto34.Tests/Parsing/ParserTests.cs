using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Canto34.Tests.Parsing
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void ShouldParseAssignmentStatements()
        {
            GetCalcParser("x = 12;").Assignment();
            GetCalcParser("x = 12 + 34;").Assignment();
            GetCalcParser("x = 12 + 34 + 56;").Assignment();
        }

        [TestMethod]
        public void ParserParsesSimpleSequence()
        {
            var parser = GetTestParser("foo bar baz");
            parser.Parse();
        }

        [TestMethod]
        public void ParserParsesFixedSequences()
        {
            var parser = GetTestParser("foo bar baz");
            var actual = parser.ParseFixedSequence();
            var expected = "barfoo";
            Assert.AreEqual(expected, actual);
        }

        private TestParser GetTestParser(string input)
        {
            var lexer = new TestLexer2(input);
            var tokens = lexer.Tokens().ToList();
            var parser = new TestParser();
            parser.Initialize(tokens, lexer);
            return parser;
        }

        private SExpressionParser GetSExprParser(string input)
        {
            var lexer = new SExpressionLexer(input);
            var tokens = lexer.Tokens().ToList();
            var parser = new SExpressionParser();
            parser.Initialize(tokens, lexer);
            return parser;
        }


        private CalcParser GetCalcParser(string input)
        {
            var lexer = new CalcLexer(input);
            var tokens = lexer.Tokens().ToList();
            var parser = new CalcParser();
            parser.Initialize(tokens, lexer);
            return parser;
        }

        [TestMethod]
        public void ParsesSExpression()
        {
            var sexpr1 = this.GetSExprParser("a").SExpression();
            var sexpr2 = this.GetSExprParser("(a b c)").SExpression();
            var sexpr3 = this.GetSExprParser("(a (b c d) c)").SExpression();
        }

        [TestMethod, ExpectedException(typeof(ParseException))]
        public void ThrowsOnUnclosedSExpressions()
        {
            this.GetSExprParser("(a").SExpression();
        }

        [TestMethod, ExpectedException(typeof(ParseException))]
        public void ThrowsOnJustACloseSExpressions()
        {
            this.GetSExprParser(")").SExpression();
        }

    }
    
    
}
