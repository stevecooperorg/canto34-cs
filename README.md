canto34-cs
==========

A c# parser-builder library

Available on [GitHub](https://github.com/stevecooperorg/canto34-cs) and as a [NuGet package](https://www.nuget.org/packages/Canto34). 

# Introducing Cant34-cs - another OSS project I probably won't maintain!

Alright, folks. This library has served as the base of language projects I've run over the years. It's called [Canto34-cs](https://github.com/stevecooperorg/canto34-cs), and it's available on both [GitHub](), where you can get the source under the very permissive MIT License, and on [NuGet](https://www.nuget.org/packages/Canto34), where it's currently on an Alpha 1 release. I've used it in a lot of places, so it's pretty useful despite the version. 

This document takes you round the basics. By the time you get to the end, you should be able to write a program to recognise simple languages. If I were you, and you wanted to get serious about learning to write languages, I'd check out [Language Implementation Patterns: Create Your Own Domain-Specific and General Programming Languages](http://pragprog.com/book/tpdsl/language-implementation-patterns) by [Terrence Parr](http://parrt.cs.usfca.edu/), the man behind [Antlr](http://www.antlr.org/). I never got on with Antlr itself, but Parr is a great communicator on the principles of writing languages, and he shows how easy it can be.

Whenever you write a language -- say, a domain-specific langugage -- you need to write a lexer and a parser. Lexers are pretty much identical the world over, and basically just need to recognise tokens; `for` and `$foo` and `int`. You do this by inheriting `Canto34.LexerBase` and making calls in the constructor, like this;

    public class SExpressionLexer : LexerBase
    {
        public const int OP = 1;
        public const int CL = 2;
        public const int ATOM = 3;

        public SExpressionLexer(string input): base(input)
        {
            this.AddLiteral(OP, "(");
            this.AddLiteral(CL, ")");
            this.AddPattern(ATOM, "[a-z]+", "atom");
            this.StandardTokens.AddWhitespace();
        }
    }

So this is a lexer which recognises three token types - an open paren, a close paren, and an 'atom' token comprising strings like `a`, `abc`, or `abbbbbbbc`. So now you can split the contents of your file into tokens like this;

    var lexer = new SExpressionLexer(input);
    var tokens = lexer.Tokens().ToList();

So now you have a list of Token objects, and you can start doing the real work to recognise what's going on inside the file. In our case, we're going to recognise lisp-style s-expressions; that means either an individual item;

    foo

or a list;

    (a b c)

or a list of lists;

    (def foo (lambda (x y) (+ x y)))

How's that achieved? A program that recognises patterns in a sequence of tokens is a parser. In Canto34, you inherit from ParserBase;

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

So look at this code. you'll see the parser has one method -- SExpression() -- which recognises an s-expression. There are two basic ideas here. One, look at the next token. Here, `LA1` is a class-level property representing the next token -- `LA1` stands for "look ahead 1 token". By looking at the next token in the stream, you can perform appropriate logic; "if the next token is an open bracket, start a new list", or "if the next token is `for`, start looking for a for-loop." 

The second concept is matching a token -- the `Match(tokenType)` method looks at the next token; if the next token isn't the expected one, an exception is thrown, indicating a syntax error. But if it's the right type, it returns the token and advances one token in the file. 

So let's say you're trying to match expressions like 

    x = 3;
    y = 10;

The pattern is;

    assignment:
      ID, EQUALS, NUMBER, SEMICOLON;

And your parser will have code like this;

    public void Assignment()
    {
        var nameToken = Match(CalcLexer.ID);
        Match(CalcLexer.EQUALS);
        var numberToken = Match(CalcLexer.NUMBER);
        Match(CalcLexer.SEMI);
        Console.WriteLine("Recognised {0} = {1}", nameToken.Content, numberToken.Content);
    }

See how every call to Match() is advancing through the file? As we keep matching tokens, we'll move from the start of the file to the end, reading and interpreting the file as we go. 

The last piece of the puzzle is calling parsing methods from inside other parsing methods. This is where the real power of this kind of parser comes from. Let's take that assignment method and make it more powerful. Let's say we want to handle both straight assignments, and addition expressions, like this;

    x = 4;
    y = 3 + 4;
    y = 1 + 2 + 3 + 4;

We first need to recognise these open-ended maths expressions like "1 + 2 + 3 + 4". We can define the language as;

    assignment:
      ID, EQUALS, additionexpression, SEMICOLON;

And our `additionexpression` is a recursively-defined expression;

     additionexpression:
         NUMBER [PLUS additionexpression];

See how that works? Either it's a number, or it's a number followed by '+' and another addition expression. 

So we define a parsing method that reflects that recursive definition, like this;

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

See how it matches the definition? read a number, look for a plus, if you see it read a number, look for a plus...

So now we can finish the job by calling this method in the original Assignment method;

    public void Assignment()
    {
        var nameToken = Match(CalcLexer.ID);
        Match(CalcLexer.EQUALS);
        var number = AdditionExpression();
        Match(CalcLexer.SEMI);
        Console.WriteLine("Recognised {0} = {1}", nameToken.Content, number);
    }

And now we can recognise everything we expect;

    x = 12; // "Recognised x = 12"
    x = 12 + 34; // "Recognised x = 46"
    x = 12 + 34 + 56; // "Recognised x = 102"

So, we're coming to the end of this intro to Canto34. Imagine the call stack when we are parsing that final expression. It's going to look like this;

    CalcParser.Assignment(); // x =
      CalcParser.AdditionExpression(); // 12 +
        CalcParser.AdditionExpression(); // 34 +
          CalcParser.AdditionExpression(); // 56

This shape that the call stack makes is what gives this style of parser it's name; a *recursive-descent* parser.
