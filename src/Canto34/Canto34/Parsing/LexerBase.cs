namespace Canto34
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Serves as a base class for creating lexers. Override and add patterns to create your lexer
    /// </summary>
    public abstract class LexerBase
    {
        private static int nextIgnoredTokenType = -1;
        private static int nextTokenType = 1;
        private List<string> literals = new List<string>();

        private LexerInput input;

        /// <summary>
        /// Contains all the token types understood by the lexer.
        /// </summary>
        private LexerPatterns patterns = new LexerPatterns();
        private Dictionary<int, List<string>> typeNameRegistry = new Dictionary<int, List<string>>();

        static LexerBase()
        {
        }

        /// <summary>
        /// Create a lexer with the string you want to scan
        /// </summary>
        /// <param name="content"></param>
        public LexerBase(string content): this(new LexerInput(content))
        {
        }

        /// <summary>
        /// Create a lexer using an existing input stream.
        /// </summary>
        /// <remarks>
        /// This is useful for systems which feature two
        /// distinct lexical structures, such as 'island
        /// grammars' where a delimiter turns one language
        /// into another - think of asp &lt;% tags
        /// delimiting HTML from VB. With this
        /// constructor, it's possible for two lexers to
        /// share a token stream.
        /// </remarks>
        /// <param name="input"></param>
        public LexerBase(LexerInput input)
        {
            this.input = input;
            this.StandardTokens = new StandardTokenTypes(this);
        }

        /// <summary>
        /// Are we at the end of the input?
        /// </summary>
        private bool eof
        {
            get { return this.input.eof; }
        }

        /// <summary>
        /// Register a new word token, like 'for' or 'where'. This will make sure the work appears within a word boundary.
        /// </summary>
        /// <param name="tokenType"></param>
        /// <param name="keyword"></param>
        public void AddKeyword(int tokenType, string keyword)
        {
            // make sure this is actually a 'word' 
            if (!Regex.IsMatch(keyword, @"^\w+$"))
            {
                throw new Exception("Sorry, but '" + keyword + "' is not a word; maybe use 'AddLiteral()' instead?");
            }

            var keywordPattern = @"\b" + Regex.Escape(keyword) + @"\b";
            AddPattern(tokenType, keywordPattern, RegexOptions.None, keyword, escapeFunction: null);

            this.literals.Add(keyword);
        }

        /// <summary>
        /// Register a new literal token type, like '-' or '#'.
        /// </summary>
        /// <param name="tokenType"></param>
        /// <param name="literal"></param>
        public void AddLiteral(int tokenType, string literal)
        {
            var keywordPattern = Regex.Escape(literal);
            AddPattern(tokenType, keywordPattern, RegexOptions.None, literal, escapeFunction: null);

            this.literals.Add(literal);
        }
        
        /// <summary>
        /// Register a new token type
        /// </summary>
        /// <param name="tokenType">the token type number</param>
        /// <param name="pattern">the regular expression for the token</param>
        /// <param name="typeName">a friendly name describing the token type</param>
        public void AddPattern(int tokenType, string pattern, string typeName)
        {
            AddPattern(tokenType, pattern, RegexOptions.None, typeName, escapeFunction:null);
        }

        /// <summary>
        /// Register a new token type
        /// </summary>
        /// <param name="tokenType">the token type number</param>
        /// <param name="pattern">the regular expression for the token</param>
        /// <param name="options">Regular expression options that should be applied to the pattern</param>
        /// <param name="typeName">a friendly name describing the token type</param>
        public void AddPattern(int tokenType, string pattern, RegexOptions options, string typeName)
        {
            AddPattern(tokenType, pattern, options, typeName, escapeFunction:null);
        }
        public class StandardTokenTypes
        {
            private readonly LexerBase lexer;
            
            public StandardTokenTypes(LexerBase lexer)
            {
                this.lexer = lexer;
            }
            public void AddWhitespace()
            {
                this.lexer.AddPattern(-99, @"\s+", "whitespace");
            }
        }

        public StandardTokenTypes StandardTokens { get; private set; }

        public void AddPattern(int tokenType, string pattern, RegexOptions options, string typeName, Func<string,string> escapeFunction)
        {
            // "\G" indicates that the pattern must match right at the start of the string.
            var modifiedPattern = "\\G" + pattern;
                                
            var rx = new Regex(modifiedPattern, options);
            var hidden = tokenType < 0; // use token types < 0 to indicate hidden tokens lie WS.
            var tokenInfo = new TokenTypeInfo(tokenType, rx, typeName, hidden, escapeFunction);
            patterns.Add(tokenInfo);

            if (!typeNameRegistry.ContainsKey(tokenType))
            {
                this.typeNameRegistry.Add(tokenType, new List<string>());
            }

            typeNameRegistry[tokenType].Add(typeName);
        }

        public string NameOfTokenType(int tokenType)
        {
            List<string> results;
            if (!this.typeNameRegistry.TryGetValue(tokenType, out results))
            {
                return "(unknown)";
            }
            else
            {
                return string.Join(" or ", results);
            }
        }

        /// <summary>
        /// get a single token from the input and move the cursor forward. If we are at the end of the input, return null. Throws an exception if no viable token can be found.
        /// </summary>
        /// <returns></returns>
        public Token NextToken()
        {
            // signal the end of the token stream.
            if (eof) { return null; }

            // check each of the patterns in turn to see if the token matches
            foreach (var tokenInfo in patterns)
            {
                var match = tokenInfo.Regex.Match(this.input.content, this.input.current);
                if (match.Success)
                {
                    // we've found our token!

                    // this is everything matched;
                    var allMatched = match.Value;

                    // this is an opportunity to get only part of the match at the token content; eg, stripping quotes from a string token.
                    var tokenText = match.Groups["content"].Success ? match.Groups["content"].Value : match.Value;

                    // apply an appropriate escape function, if necessary
                    if (tokenInfo.EscapeFunction != null)
                    {
                        tokenText = tokenInfo.EscapeFunction(tokenText);
                    }

                    // save these for later
                    this.input.tokenNumber++;
                    var token = new Token(tokenText, tokenInfo, input.line, input.character, input.tokenNumber);

                    // Now that we've matched a token, we need to advance the cursor and the tracking of the line and character.
                    input.Advance(allMatched);

                    // return the successful token.
                    return token;
                }
            }

            // could not match one of the regular expressions. Throw a helpful exception...
            var piece = input.Next(20);

            var specialCharNames = new Dictionary<char,string>()
            {
                { ' ', "space" },
                { '\t', "tab" },
                { '\r', "CR" },
                { '\n', "LF" },
            };

            System.Func<char, string> translate = c => {
                string result = "";
                return specialCharNames.TryGetValue(c, out result) ? result : c.ToString();
            };

            var errorMessage = string.Format("No viable token at line {0} character {1}: '{2}...' (chars [{3}])", input.line, input.character, piece, string.Join(", ", piece.ToCharArray().Select(c => translate(c))));
            throw new Exception(errorMessage);
        }

        /// <summary>
        /// Enumerate all the tokens in the input string
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Token> Tokens()
        {
            var tok = NextToken();
            while (tok != null)
            {
                // give the non-hidden tokens back.
                if (!tok.Type.Hidden)
                {
                    yield return tok;
                }
                tok = NextToken();
            }
        }

        /// <summary>
        /// For simple parsers that use only strings, return just the token content.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> TokenValues()
        {
            foreach (var token in Tokens())
            {
                yield return token.Content;
            }
        }

        protected static int NextIgnoredTokenType()
        {
            return nextIgnoredTokenType--;
        }

        protected static int NextTokenType()
        {
            return nextTokenType++;
        }
    }

    public sealed class LexerInput
    {
        public int character = FIRSTCHAR;

        // the content we're lexing.
        public string content;

        // the current cursor position.
        public int current;

        // these are used to track token positions in friendly line/character coordinates
        public int line = FIRSTLINE;

        // incremental count of token number
        public int tokenNumber = 0;

        private const int FIRSTCHAR = 1;

        // Is the very first character on line 0 or 1? Character 0 or 1?
        private const int FIRSTLINE = 1;

        public LexerInput(string content)
        {
            this.content = content;
            this.current = 0;
        }

        public bool eof
        {
            get { return this.current >= this.content.Length; }
        }

        internal void Advance(string allMatched)
        {
            foreach (var c in allMatched)
            {
                character++;
                current++;
                if (c == '\n')
                {
                    // we've moved on a line; set the line and character positions
                    line++;
                    character = FIRSTCHAR;
                }
            }
        }

        internal string Next(int numberOfCharacters)
        {
            var maxPieceLength = Math.Min(numberOfCharacters, this.content.Length - this.current);
            var piece = this.content.Substring(this.current, maxPieceLength);
            return piece;
        }
    }

    internal class LexerPatterns : List<TokenTypeInfo>
    {
    }
}