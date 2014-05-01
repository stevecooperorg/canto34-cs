namespace Canto34
{
    using System;

    public class ParseException : Exception
    {
        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException(Token LA1, string message)
            : base(string.Format("({0},{1}) {2}: {3}", LA1.Line, LA1.Character, LA1.ToString(), message))
        {
        }

        public override string StackTrace
        {
            get
            {
                return base.StackTrace;
            }
        }
    }
}