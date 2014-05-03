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

    public class CalcParser : ParserBase
    {
        public void Assignment()
        {
            var nameToken = Match(CalcLexer.ID);
            Match(CalcLexer.EQUALS);
            var number = AdditionExpression();
            Match(CalcLexer.SEMI);
            Console.WriteLine("Recognised {0} = {1}", nameToken.Content, number);
        }

        public int AdditionExpression()
        {
            // get the number
            var numberToken = Match(CalcLexer.NUMBER);
            var number = int.Parse(numberToken.Content, CultureInfo.InvariantCulture);

            // look for a plus; recurse.
            if (!EOF && LA1.Is(CalcLexer.PLUS))
            {
                Match(CalcLexer.PLUS);
                number += AdditionExpression();
            }

            return number;
        }
    }

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

    public class SExpressionParser : ParserBase
    {
        internal dynamic SExpression()
        {
            if (LA1.Is(SExpressionLexer.OP))
            {
                Match(SExpressionLexer.OP);
            }
            else
            {
                var atom = Match(SExpressionLexer.ATOM).Content;
                return atom;
            }

            var array = new List<dynamic>();
            while (!EOF && !LA1.Is(SExpressionLexer.CL))
            {
                array.Add(SExpression());
            }

            Match(SExpressionLexer.CL);
            return array;
        }
    }

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

    public class TestParser : ParserBase
    {
        public TestParser()
        {

        }

        internal void Parse()
        {
            Match(1);
            Match(2);
            Match(3);
        }

        internal string ParseFixedSequence()
        {
            return Match(1, 2, (a, b) => b + a);
        }

        //internal string ParsePushdownSequence()
        //{
        //    return Match(1, Match , 1, (a, b) => b + a);
        //}


    }
}
