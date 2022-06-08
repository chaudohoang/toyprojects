using System;
using System.IO;

namespace Program
{
	class Program
	{
		public static void Main(string[] args)
		{
			var rootDir = Directory.GetCurrentDirectory();
			var fileNames = Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories);
			foreach(String path in fileNames)
			{
			    var dir = Path.GetDirectoryName(path);
				var extraString = new DirectoryInfo(dir).Name.Replace("PDF","");
				var fileName = extraString + Path.GetFileNameWithoutExtension(path);
				var ext = Path.GetExtension(path);
			    var newPath = Path.Combine(dir, fileName+ext);
				if (ext == ".pdf")
				{
					File.Move(path, newPath);
				}			    
			}
		}
	}
}