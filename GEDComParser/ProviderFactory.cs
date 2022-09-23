using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEDComParser
{
    public class ProviderFactory
    {
        public static IParser GetParserProvider()
        {
            return new GDCParser();
        }
    }
}
