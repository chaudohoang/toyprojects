using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCloner
{
    internal class CloneCppConsoleProject
    {
        public static string CloneCppProject(string originalPath, string inputFolder)
        {
            // Extract original project name from the original path's folder name
            string originalProjectName = Path.GetFileName(originalPath);

            // Get the parent folder of the original project
            string parentFolder = Directory.GetParent(originalPath).FullName;

            // Extract new project name from .h file in the input folder
            string newProjectName = GetProjectNameFromHeader(inputFolder);

            // Create new project path
            string newPath = Path.Combine(parentFolder, newProjectName);

            // Ensure the new project directory exists
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            try
            {
                // Copy the entire folder structure from originalPath to newPath
                CopyFolderStructure(originalPath, newPath);

                // Replace lib files, header files, and strings in .cpp files
                ReplaceLibAndHeaderFiles(newPath, inputFolder);
                ReplaceStringsInCppFiles(newPath, originalProjectName, newProjectName);

                // Rename and modify solution file, project file, and project filters file
                RenameAndModifyFiles(newPath, originalProjectName, newProjectName);
                return newPath;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }         
                        
        }

        private static void RenameAndModifyFiles(string projectPath, string originalProjectName, string newProjectName)
        {
            // Rename and modify solution file
            RenameAndModifyFile(projectPath, $"{originalProjectName}.sln", $"{newProjectName}.sln", originalProjectName, newProjectName);

            // Rename and modify project file
            RenameAndModifyFile(projectPath, $"{originalProjectName}.vcxproj", $"{newProjectName}.vcxproj", originalProjectName, newProjectName);

            // Rename and modify project filters file
            RenameAndModifyFile(projectPath, $"{originalProjectName}.vcxproj.filters", $"{newProjectName}.vcxproj.filters", originalProjectName, newProjectName);
        }

        private static void RenameAndModifyFile(string projectPath, string oldFileName, string newFileName, string oldString, string newString)
        {
            // Rename the file
            RenameFile(Path.Combine(projectPath, oldFileName), Path.Combine(projectPath, newFileName));

            // Modify the content of the file
            ReplaceStringInFile(Path.Combine(projectPath, newFileName), oldString, newString);
        }


        private static void RenameFile(string oldPath, string newPath)
        {
            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
            }
        }

        private static void ReplaceStringInFile(string filePath, string oldString, string newString)
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                content = content.Replace(oldString, newString);
                File.WriteAllText(filePath, content);
            }
        }

        private static void CopyFolderStructure(string sourceFolder, string destinationFolder)
        {
            // Create the destination directory if it doesn't exist
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Copy all files and subfolders, excluding .user files
            foreach (var sourceItem in Directory.GetFileSystemEntries(sourceFolder).Where(f => !f.EndsWith(".user", StringComparison.OrdinalIgnoreCase)))
            {
                string destinationItem = Path.Combine(destinationFolder, Path.GetFileName(sourceItem));

                if (Directory.Exists(sourceItem))
                {
                    // Recursively copy subfolders
                    CopyFolderStructure(sourceItem, destinationItem);
                }
                else
                {
                    // Copy files
                    File.Copy(sourceItem, destinationItem, true);
                }
            }
        }


        private static void ReplaceLibAndHeaderFiles(string destinationFolder, string inputFolder)
        {
            // Find the subfolder containing lib files
            var libSubfolder = FindSubfolder(destinationFolder, "*.lib");

            if (libSubfolder != null)
            {
                ReplaceFiles(libSubfolder, inputFolder, "*.lib");
            }

            // Find the subfolder containing header files
            var headerSubfolder = FindSubfolder(destinationFolder, "*.h");

            if (headerSubfolder != null)
            {
                ReplaceFiles(headerSubfolder, inputFolder, "*.h");
            }
        }

        private static void ReplaceStringsInCppFiles(string projectPath, string oldString, string newString)
        {
            // Find the subfolder containing .cpp files
            var cppSubfolder = FindSubfolder(projectPath, "*.cpp");

            if (cppSubfolder != null)
            {
                // Replace strings in .cpp files
                var cppFiles = Directory.GetFiles(cppSubfolder, "*.cpp");

                foreach (var cppFile in cppFiles)
                {
                    ReplaceStringInFile(cppFile, oldString, newString);
                }
            }
        }

        private static string FindSubfolder(string parentFolder, string searchPattern)
        {
            // Search for subfolders containing files matching the search pattern
            var subfolders = Directory.GetDirectories(parentFolder);

            foreach (var subfolder in subfolders)
            {
                if (Directory.GetFiles(subfolder, searchPattern).Any())
                {
                    return subfolder;
                }
            }

            return null; // No subfolder found
        }

        private static void ReplaceFiles(string destinationFolder, string inputFolder, string searchPattern)
        {
            // Delete all existing files in the destination folder
            foreach (var file in Directory.GetFiles(destinationFolder, searchPattern, SearchOption.AllDirectories))
            {
                File.Delete(file);
            }

            // Copy files from the input folder to the destination folder
            foreach (var inputFile in Directory.GetFiles(inputFolder, searchPattern))
            {
                var destinationFile = Path.Combine(destinationFolder, Path.GetFileName(inputFile));
                File.Copy(inputFile, destinationFile);
            }
        }


        private static string GetProjectNameFromHeader(string path)
        {
            var headerFiles = Directory.GetFiles(path, "*.h", SearchOption.AllDirectories);

            if (headerFiles.Length > 0)
            {
                // Extract project name from the first header file name
                string headerFileName = Path.GetFileNameWithoutExtension(headerFiles[0]);
                return headerFileName; // You might need additional processing based on your file naming conventions
            }

            // Default to a generic name if not found
            return "DefaultProjectName";
        }

        
    }
}
