using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryptandDecrypt
{

    class StringManip
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

    }
}

