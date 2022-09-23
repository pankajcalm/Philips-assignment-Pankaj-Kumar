using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GEDComParser
{
    public interface IParser
    {
        XmlDocument Process(string fileName);

        string Save(string xmlData, string exportDir);
    }
}
