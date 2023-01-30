using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BinXMLChangeIntercept
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string intercept1,intercept2;

            Console.Write("Enter Intercept 1 : ");
            intercept1 = Console.ReadLine();
            Console.Write("Enter Intercept 2 : ");
            intercept2 = Console.ReadLine();

            XmlNode node;
            XmlNodeList nodes;
            var xmlDoc = new XmlDocument();

            string[] files = Directory.GetFiles(@"C:\Radiant Vision Systems Data\TrueTest\AppData", "*.xml");

            foreach (string file in files)
            {
                if (file.Contains("RBin") || file.Contains("GBin") || file.Contains("BBin"))
                {

                    xmlDoc.Load(file);

                    nodes = xmlDoc.DocumentElement.SelectNodes("/Y29CCD/Items/CCDLineCalibration/LineList/CCDLine/R1/Intercept");
                    for (int index = 0; index <= nodes.Count - 1; index++)
                    {
                        nodes[index].InnerText = intercept1;
                    }

                    nodes = xmlDoc.DocumentElement.SelectNodes("/Y29CCD/Items/CCDLineCalibration/LineList/CCDLine/R2/Intercept");
                    for (int index = 0; index <= nodes.Count - 1; index++)
                    {
                        nodes[index].InnerText = intercept1;
                    }

                    XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                    XmlWriter writer = XmlWriter.Create(file, settings);
                    xmlDoc.Save(writer);
                    if (writer != null)
                        writer.Close();

                }
            }
            Console.WriteLine("DONE ...");
            Console.Read();
        }
    }
}
