using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canto34.Tests.Parsing
{
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
}
