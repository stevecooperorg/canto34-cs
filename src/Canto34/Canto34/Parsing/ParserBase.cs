namespace Canto34
{
    using System;
    using System.Collections.Generic;
    using Canto34.Utils;

    /// <summary>
    /// Serves as a base for constructing parsers. Includes basic parse machinery like matching
    /// </summary>
    public abstract class ParserBase
    {
        private int current = 0;
        private bool Initialized = false;
        private LexerBase lexer;
        private List<Token> Tokens;
        Stack<int> rewindPoints = new Stack<int>(); 

        public ParserBase()
        {
        }

        /// <summary>
        /// Either null, or the path to the script being parsed.
        /// </summary>
        public string ScriptFilePath
        {
            get; set;
        }

        protected bool EOF
        {
            get
            {
                EnsureInitialized();
                return LA1 == null;
            }
        }

        protected Token LA1
        {
            get
            {
                EnsureInitialized();
                if (current >= Tokens.Count) { return null; }
                else { return Tokens[current]; }
            }
        }

        protected void Rewind()
        {
            this.current = rewindPoints.Pop();
        }

        protected void SetRewindPoint()
        {
            rewindPoints.Push(this.current);
        }
        public ParseException Error(string message)
        {
            message = StackTraceTools.BreadcrumbTrail(this.GetType()) + " : " + message;
            try
            {
                if (!EOF)
                {
                    throw new ParseException(this.LA1, message);
                }
                else
                {
                    throw new ParseException(message);
                }
            }
            catch (ParseException ex)
            {
                ex.ImpersonateErrorInExternalFile(this.ScriptFilePath, 1);
                throw;
            }
            throw new InvalidOperationException("Whoops! We should never have got here, but we need the line so the compiler doesn't warn about a missing return.");
        }

        public ParseException Error(Token LA1, string message)
        {
            message = string.Format("Line {0} Char {1}: {2} : {3}" , LA1.Line, LA1.Character, StackTraceTools.BreadcrumbTrail(this.GetType()), message);
            try
            {
                throw new ParseException(LA1, message);
            }
            catch (ParseException ex)
            {
                ex.ImpersonateErrorInExternalFile(this.ScriptFilePath, 1);
                throw;
            }
            throw new InvalidOperationException("Whoops! We should never have got here, but we need the line so the compiler doesn't warn about a missing return.");
        }

        public virtual void Initialize(List<Token> tokens, LexerBase lexer)
        {
            this.Tokens = tokens;
            this.current = 0;
            this.Initialized = true;
            this.lexer = lexer;
        }

        protected bool CanMatch(string content)
        {
            return LA1 != null && LA1.Content == content;
        }

        protected bool CanMatch(int type)
        {
            return LA1 != null && LA1.TokenType == type;
        }

        protected Token Match(string expected)
        {
            var actual = MatchAny();
            if (actual == null)
            {
                throw this.Error(actual, "EOF found but expected '" + actual + "'");
            }
            else if (expected != actual.Content)
            {
                throw this.Error(actual, "'" + actual.Content + "' found but expected '" + expected + "'");
            }
            return actual;
        }

        protected Token Match(int expected)
        {
            var actual = MatchAny();
            if (actual == null)
            {
                throw Error(actual, "EOF found but expected '" + actual + "'");
            }
            else if (expected != actual.TokenType)
            {
                var msg = string.Format("({0}, {1}): found token '{2}' but expected '{3}'.", actual.Line, actual.Character, actual.ToString(), this.lexer.NameOfTokenType(expected));
                throw Error(actual, msg);
            }
            return actual;
        }

        protected Token MatchAny()
        {
            EnsureInitialized();
            if (LA1 == null)
            {
                throw Error("EOF found but expected some content");
            }
            var currentToken = LA1;
            current++;
            return currentToken;
        }

        protected Guid MatchGuid()
        {
            var tok = MatchAny();
            try
            {
                var guid = new Guid(tok.Content);
                return guid;
            }
            catch
            {
                throw Error(tok, "Expecting a guid but encountered " + tok.ToString());
            }
        }

        private void EnsureInitialized()
        {
            if (!Initialized) { throw new InvalidOperationException("The parser is not yet initialized"); }
        }
    }
}