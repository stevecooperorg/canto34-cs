namespace Canto34.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class StackTraceTools
    {
        internal static string BreadcrumbTrail(Type type)
        {
            // finds all the stack trace frames in the given type;
            StackTrace st = new StackTrace();
            var methodNames = st.GetFrames()
                .SkipWhile(f => f.GetMethod().DeclaringType != type)
                .TakeWhile(f => f.GetMethod().DeclaringType == type)
                .Select( f=> f.GetMethod().Name)
                .ToList();

            methodNames.Reverse();

            var methodNameList = string.Join(" > ", methodNames);

            return methodNameList;
        }
    }
}