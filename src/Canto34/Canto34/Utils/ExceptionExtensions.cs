namespace Canto34.Utils
{
    using System;

    /// <summary>
    /// Extension methods for exceptions.
    /// </summary>
    public static class ExceptionExtensions
    {
        public static void ImpersonateErrorInExternalFile(this Exception ex, string file, int lineNumber)
        {
            // todo; rewrite the stack trace to show that the error originates inside
            // an external file (such as DSL source code) rather than in the
            // interpreter.

        }
    }
}