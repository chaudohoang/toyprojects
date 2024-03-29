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
				var fileName = "Public - " + Path.GetFileNameWithoutExtension(path);
				var ext = Path.GetExtension(path);
			    var newPath = Path.Combine(dir, fileName+ext);
			    File.Move(path, newPath);
			}
		}
	}
}