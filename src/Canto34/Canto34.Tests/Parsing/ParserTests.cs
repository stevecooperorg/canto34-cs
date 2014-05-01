﻿using System;
using System.Collections.Generic;
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

        [TestMethod]
        public void ParsesSExpression()
        {
            var sexpr1 = this.GetSExprParser("a").SExpression();
            var sexpr2 = this.GetSExprParser("(a b c)").SExpression();
            var sexpr3 = this.GetSExprParser("(a (b c d) c)").SExpression();
        }
    }

    public class SExpressionLexer : LexerBase
    {
        public SExpressionLexer(string input): base(input)
        {
            this.AddLiteral(1, "(");
            this.AddLiteral(2, ")");
            this.AddPattern(3, "[a-z]+", "atom");
            this.StandardTokens.AddWhitespace();
        }
    }

    public class SExpressionParser : ParserBase
    {
        public SExpressionParser()
        {

        }

        internal dynamic SExpression()
        {
            if (LA1.Is("("))
            {
                Match("(");
            }
            else
            {
                var atom = MatchAny().Content;
                return atom;
            }

            var array = new List<dynamic>();
            while (!LA1.Is(")"))
            {
                array.Add(SExpression());
            }

            Match(")");
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