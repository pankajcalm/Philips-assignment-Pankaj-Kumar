using Microsoft.VisualStudio.TestTools.UnitTesting;
using GEDComParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GEDComParser.Tests
{
    [TestClass()]
    public class GDCParserTests
    {

        [TestMethod()]
        public void SaveTest()
        {
            GDCParser gDCParser = new GDCParser();

            string FileNameWPath = Path.GetFullPath(@"\import\Genealogical_Data_communication_file_sample_data.txt");

            var xmlData = gDCParser.Process(FileNameWPath);

            Assert.IsNotNull(xmlData);

            Assert.IsTrue(!string.IsNullOrEmpty(xmlData.OuterXml));

        }

        [TestMethod()]
        public void ProcessTest()
        {
            string xmlData = $"<gedcom><indi id={"\"@l1@\""}><name value=\"Jamis Gordon /Buck/\" ><surn>Buck</surn><givn>Jamis Gordon</givn></name><sex>M</sex></indi></gedcom> ";
            GDCParser gDCParser = new GDCParser();

            string filename = gDCParser.Save(xmlData, @"export");

            Assert.IsTrue(!string.IsNullOrEmpty(filename));


        }
    }
}