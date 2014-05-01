namespace Canto34
{
    using System;
    using System.Linq;

    /// <summary>
    /// A basic token, with lex info such as the line and character the token was found at.
    /// </summary>
    public class Token
    {
        public readonly int Character;
        public readonly string Content;
        public readonly int Line;
        public readonly int TokenNumber;
        public readonly TokenTypeInfo Type;

        public Token(string content, TokenTypeInfo type, int line, int character, int tokenNumber)
        {
            this.Character = character;
            this.Line = line;
            this.Content = content;
            this.Type = type;
            this.TokenNumber = tokenNumber;
        }

        public int TokenType
        {
            get { return Type.TokenType; }
        }

        public string TypeName
        {
            get { return Type.TypeName; }
        }

        public bool Is(string content)
        {
            return this.Content == content;
        }

        public bool Is(int tokenType)
        {
            return this.TokenType == tokenType;
        }

        public bool IsOneOf(params int[] tokenTypes)
        {
             return tokenTypes.Any(tokenType => this.TokenType == tokenType);
        }

        public override string ToString()
        {
            return string.Format("'{0}' ({1})", this.Content, this.TypeName);
        }

        //public static bool operator ==(Token token, string str)
        //{
        //    return token.Content == str;
        //}
        //public static bool operator !=(Token token, string str)
        //{
        //    return (token == str) == false;
        //}
        //public static bool operator ==(Token token, int i)
        //{
        //    return (token != null && token.TokenType == i);
        //}
        //public static bool operator !=(Token token, int i)
        //{
        //    return (token == i) == false;
        //}
    }
}