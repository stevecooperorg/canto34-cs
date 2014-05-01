namespace Canto34
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A particular type of token, such as Whitespace or Identifier.
    /// </summary>
    public class TokenTypeInfo
    {
        /// <summary>An optional escape function which transforms the token content before it is returned from the lexer.</summary>
        public readonly Func<string, string> EscapeFunction;

        /// <summary>Should the token type be hidden from parsers? Eg, whitespace.</summary>
        public readonly bool Hidden;

        /// <summary>The regular expression matching this type of token</summary>
        public readonly Regex Regex;

        /// <summary>An identifying integral type. Several token types can share the same integer if, say, there are several patterns to describe a string.</summary>
        public readonly int TokenType;

        /// <summary>a friendly name for the token type</summary>
        public readonly string TypeName;

        public TokenTypeInfo(int tokenType, Regex regex, string typeName, bool hidden, Func<string, string> escapeFunction)
        {
            this.TokenType = tokenType;
            this.Regex = regex;
            this.TypeName = typeName;
            this.Hidden = hidden;
            this.EscapeFunction = escapeFunction;
        }
    }
}