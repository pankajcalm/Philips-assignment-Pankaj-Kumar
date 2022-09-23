using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEDComParser
{
    class Program
    {
        const string DEFAULTIMPORTFILE_KEY = "DefaultImportFile";
        const string EXPORTPATH_LOCATION_KEY = "ExportPath";
        static void Main(string[] args)
        {
            try
            {


                //Get File Name
                string sourceFile = ConfigurationManager.AppSettings.GetValues(DEFAULTIMPORTFILE_KEY).FirstOrDefault();
                Console.WriteLine($"Source File is: {Path.GetFullPath(sourceFile)}");

                Console.WriteLine($"Do you want to Continue with this selection? Enter y for yes Or n for No:  y / n ?");

                if (Console.ReadLine().Equals("n"))
                {
                    Console.WriteLine("Enter File Name complete path:");
                    string tempFile = Console.ReadLine();
                    if (File.Exists(tempFile))
                        sourceFile = tempFile;
                    else
                    {
                        Console.WriteLine("File not available! Enter Valid File:");
                        tempFile = Console.ReadLine();
                        if (File.Exists(tempFile))
                            sourceFile = tempFile;
                        else
                        {
                            Console.WriteLine("Try For Next Time!");
                            Console.ReadKey();
                            return;
                        }
                    }
                }

                IParser gdcParser = ProviderFactory.GetParserProvider();

                var xmlDoc = gdcParser.Process(Path.GetFullPath(sourceFile));

                string exportDir = ConfigurationManager.AppSettings.GetValues(EXPORTPATH_LOCATION_KEY).FirstOrDefault();

                gdcParser.Save(xmlDoc.OuterXml, exportDir);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }
}
