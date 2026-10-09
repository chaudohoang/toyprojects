using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RemoteToolsTabbed
{
    /// <summary>
    /// The machine list, a plain two column CSV: name,address - no header row.
    /// Several of them are loaded at once and merged into one collection, so every row
    /// remembers the <see cref="MachineList"/> it came from and is saved back to that file.
    /// </summary>
    internal static class ComputerList
    {
        /// <summary>Reads one CSV, tagging every row with the list it came from.</summary>
        public static List<Computer> Load(MachineList source)
        {
            List<Computer> list = new List<Computer>();
            if (source == null || !File.Exists(source.Path))
                return list;

            foreach (string rawLine in File.ReadAllLines(source.Path))
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                string[] fields = line.Split(',');
                string name = fields[0].Trim();
                string address = fields.Length > 1 ? fields[1].Trim() : string.Empty;

                // Tolerate a header row left behind by an older version of the tool.
                if (string.Equals(name, "pc", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(address, "ip", StringComparison.OrdinalIgnoreCase))
                    continue;

                list.Add(new Computer { Name = name, Address = address, List = source });
            }

            return list;
        }

        /// <summary>
        /// Writes back the rows belonging to <paramref name="source"/>, picked out of the whole
        /// merged collection - deliberately not what the grid happens to show, so saving while
        /// the list is filtered cannot drop the hidden machines, and rows from the other loaded
        /// CSVs cannot leak into this one.
        /// </summary>
        public static void Save(MachineList source, IEnumerable<Computer> computers)
        {
            if (source == null)
                return;

            StringBuilder sb = new StringBuilder();

            foreach (Computer computer in computers)
            {
                if (computer == null || !ReferenceEquals(computer.List, source))
                    continue;

                string name = Clean(computer.Name);
                string address = Clean(computer.Address);
                if (name.Length == 0 && address.Length == 0)
                    continue;

                sb.Append(name).Append(',').AppendLine(address);
            }

            File.WriteAllText(source.Path, sb.ToString());
        }

        private static string Clean(string value)
        {
            return (value ?? string.Empty).Replace(',', ' ').Trim();
        }
    }
}
