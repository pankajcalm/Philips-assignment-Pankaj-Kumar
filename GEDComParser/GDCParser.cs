using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GEDComParser
{
    /// <summary>
    /// GDCParser to parse into xml
    /// </summary>
    public class GDCParser : IParser
    {
        private const string ROOTNODE = "gedcom";

        /// <summary>
        /// Load text and process to XML 
        /// </summary>
        /// <param name="fileNameWithPath"></param>
        /// <returns></returns>
        public XmlDocument Process(string fileNameWithPath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);

                // Add Root Node "gdecom"
                XmlElement element1 = xmlDocument.CreateElement(string.Empty, ROOTNODE, string.Empty);
                xmlDocument.AppendChild(element1);

                XmlElement root = xmlDocument.DocumentElement;

                // Read File
                var gedcomContents = ReadFile(fileNameWithPath);

                int index = 0;
                while (index < gedcomContents.Length)
                {
                    //1 NOTE @N0001@
                    string tagOrId, data;
                    int currentLvl = 0;
                    string dataline = gedcomContents[index];

                    GetNodeTagData(gedcomContents, out currentLvl, gedcomContents, index, out dataline, out tagOrId, out data);

                    string id = string.Empty;
                    if (tagOrId[0] == '@' && tagOrId[tagOrId.Length - 1] == '@')
                        id = tagOrId;

                    XmlElement subtree = null;
                    XmlElement nameSubtree = null;
                    XmlElement famSubtree = null;
                    XmlElement childNameSubtree = null;

                    // This starts a new subtree of type
                    subtree = GetZeroXmlNode(id, string.IsNullOrEmpty(id) ? tagOrId : data, xmlDocument);

                    currentLvl++;
                    while (currentLvl > 0)
                    {
                        index++;
                        GetNodeTagData(gedcomContents, out currentLvl, gedcomContents, index, out dataline, out tagOrId, out data);
                        if (currentLvl == 0)
                            break;

                        if (currentLvl == 1)
                        {
                            string famName = string.Empty;
                            int startIndex = data.IndexOf('/');
                            int lastIndex = data.LastIndexOf('/');

                            if (startIndex > -1)
                                famName = data.Substring(startIndex, lastIndex - startIndex + 1);

                            if (!string.IsNullOrEmpty(famName))
                                data = data.Replace(famName, "");

                            // This starts a NAME subtree with a value
                            if (nameSubtree != null)
                                subtree.AppendChild(nameSubtree);
                            nameSubtree = GetFirstXmlNode(tagOrId, data.TrimEnd(' '), xmlDocument);

                            famSubtree = GetFamilyXmlNode(tagOrId, famName.Trim('/'), xmlDocument);
                            if (nameSubtree != null && famSubtree != null)
                                subtree.AppendChild(famSubtree);
                        }
                        else
                        {
                            if (nameSubtree != null)
                            {
                                childNameSubtree = GetChildXmlNode(tagOrId, data, xmlDocument);
                                nameSubtree.AppendChild(childNameSubtree);
                            }
                        }
                    }

                    if (nameSubtree != null)
                    {
                        subtree.AppendChild(nameSubtree);
                        if (famSubtree != null)
                            subtree.AppendChild(famSubtree);
                    }
                    root.AppendChild(subtree);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return xmlDocument;
        }

        private static void GetNodeTagData(string[] gedcomContents, out int currentLevel, string[] gedcomContents1, int index, out string dataline, out string tagOrId, out string data)
        {
            currentLevel = 0;
            dataline = data = tagOrId = string.Empty;
            if (gedcomContents1.Length > index)
            {
                dataline = gedcomContents1[index];
                int firstSpaceIndex = dataline.IndexOf(' ');
                int secondSpaceIndex = dataline.IndexOf(' ', firstSpaceIndex + 1);
                if (firstSpaceIndex > 0)
                    currentLevel = Convert.ToInt32(dataline.Substring(0, firstSpaceIndex));
                if (secondSpaceIndex > 0)
                {
                    tagOrId = dataline.Substring(firstSpaceIndex + 1, secondSpaceIndex - firstSpaceIndex - 1);
                    data = dataline.Substring(secondSpaceIndex + 1);
                }
                else
                    tagOrId = dataline.Substring(firstSpaceIndex + 1);
            }
        }

        private XmlElement GetZeroXmlNode(string id, string data, XmlDocument doc)
        {
            XmlElement result = doc.CreateElement(data);

            //XmlElement node = doc.CreateElement(data);
            XmlAttribute xmlAttribute = doc.CreateAttribute("", "id", "");
            xmlAttribute.Value = id;
            result.Attributes.Append(xmlAttribute);

            return result;
        }

        /// <summary>
        /// This starts a NAME subtree with a value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private XmlElement GetFirstXmlNode(string name, string value, XmlDocument doc)
        {
            XmlElement result = doc.CreateElement(name);
            if (name.Equals("name", StringComparison.InvariantCultureIgnoreCase))
            {
                result.SetAttribute("value", value);
            }
            else
                result.InnerText = value;


            return result;
        }

        private XmlElement GetFamilyXmlNode(string name, string value, XmlDocument doc)
        {
            XmlElement result = null;
            if (name.Equals("name", StringComparison.InvariantCultureIgnoreCase))
            {
                result = doc.CreateElement("FAMNAME");
                result.InnerText = value;
            }

            return result;
        }


        private XmlElement GetChildXmlNode(string name, string data, XmlDocument doc)
        {
            XmlElement result = doc.CreateElement(name);

            result.InnerText = data;

            return result;
        }

        private string[] ReadFile(string fileNameWithPath)
        {
            string[] content = null;

            if (File.Exists(fileNameWithPath))
                content = File.ReadAllLines(fileNameWithPath);
            else
                Console.WriteLine($"File does not exists:{fileNameWithPath}");

            return content;
        }

        /// <summary>
        /// Save Xml Data into a File at configured location
        /// </summary>
        /// <param name="xmlData">XML</param>
        /// <param name="exportDir"></param>
        public string Save(string xmlData, string exportDir)
        {
            string exportPath = string.Empty;
            try
            {
                string path = Path.GetFullPath(exportDir);
                exportPath = Path.Combine(path, $"OutputXml_{DateTime.Now.ToString("ddMMyyyHHmmss")}.xml");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.WriteAllText(exportPath, xmlData);

                Console.WriteLine($"File generated successfully at \n {exportPath}");
            }
            catch (Exception ex)
            {
                exportPath = "";
                throw ex;
            }

            return exportPath;
        }
    }
}
