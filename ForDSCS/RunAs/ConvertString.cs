using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace RunAs
{
    class ConvertString
    {
        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes
                  = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue
                  = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        static public string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes
                = System.Convert.FromBase64String(encodedData);
            string returnValue =
               System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
        static public void EncodeFileTo64(string sourcefile, string desfile)
        {
            string[] lines = System.IO.File.ReadAllLines(sourcefile);
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(desfile))
                for (int i = 0; i <= lines.Length - 1; i++)
                {
                    lines[i] = EncodeTo64(lines[i]);

                    sw.WriteLine(lines[i]);
                }

        }
        static public void DecodeFileFrom64(string sourcefile, string desfile)
        {
            string[] lines = System.IO.File.ReadAllLines(sourcefile);
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(desfile))
                for (int i = 0; i <= lines.Length - 1; i++)
                {
                    lines[i] = DecodeFrom64(lines[i]);

                    sw.WriteLine(lines[i]);
                }


        }

        public static string calcSHA256(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();
            }
        }

        static public string FormatFilePath(string path)
        {
            return path.Replace("\"", ""); ;
        }

        public static bool IsNetworkPath(string path)
        {
            if (!path.StartsWith(@"/") && !path.StartsWith(@"\"))
            {
                string rootPath = System.IO.Path.GetPathRoot(path); // get drive's letter
                if (rootPath == "") return false;
                System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(rootPath); // get info about the drive
                return driveInfo.DriveType == System.IO.DriveType.Network; // return true if a network drive
            }

            return true; // is a UNC path
        }
    }
}
