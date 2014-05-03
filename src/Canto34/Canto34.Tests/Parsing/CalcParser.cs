using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canto34.Tests.Parsing
{
    public class CalcParser : ParserBase
    {
        public void Assignment()
        {
            var nameToken = Match(CalcLexer.ID);
            Match(CalcLexer.EQUALS);
            var number = AdditionExpression();
            Match(CalcLexer.SEMI);
            Console.WriteLine("Recognised {0} = {1}", nameToken.Content, number);
        }

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
    }
}
