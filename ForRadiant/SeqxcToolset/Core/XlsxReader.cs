using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace SeqxcToolset.Core
{
    /// <summary>
    /// One worksheet's cells, as text.
    /// </summary>
    public class XlsxSheet
    {
        public string Name;

        /// <summary>
        /// Excel row number (1-based, exactly as shown in Excel) to that row's cells,
        /// keyed by 0-based column index. Both levels are sparse: a workbook stores
        /// only the cells that have content, and an empty cell is simply absent rather
        /// than present-and-blank. Cells are therefore addressed by their reference,
        /// never by position within the row — a row that starts at column C must not
        /// have C read as if it were A.
        /// </summary>
        public SortedDictionary<int, Dictionary<int, string>> Rows =
            new SortedDictionary<int, Dictionary<int, string>>();

        public int MaxColumnIndex = -1;

        public Dictionary<int, string> Row(int rowNumber) =>
            Rows.TryGetValue(rowNumber, out var r) ? r : null;

        public string Cell(int rowNumber, int columnIndex)
        {
            var r = Row(rowNumber);
            return r != null && r.TryGetValue(columnIndex, out var v) ? v : "";
        }
    }

    /// <summary>
    /// A deliberately small .xlsx reader, enough to pull a named sheet's cells out as
    /// text. An .xlsx is a zip of XML parts, so System.IO.Compression plus
    /// System.Xml.Linq cover it and this project keeps its zero-NuGet-dependency
    /// property — no ClosedXML/EPPlus, and no Excel install or COM interop on the
    /// machine either.
    ///
    /// Scope and limits, since this is not a general-purpose library:
    /// - Reads cell *values*, not formatting. A number formatted as a date comes back
    ///   as its serial number, not a date string.
    /// - Formula cells yield their last cached result, which is what Excel wrote when
    ///   it saved. A workbook saved by something that doesn't cache results would read
    ///   as blank there.
    /// - Numbers are rendered the way Excel displays them (see FormatNumber).
    /// </summary>
    public static class XlsxReader
    {
        private static readonly XNamespace Main =
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Rel =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PkgRel =
            "http://schemas.openxmlformats.org/package/2006/relationships";

        /// <summary>Sheet names in workbook (tab) order.</summary>
        public static List<string> GetSheetNames(string path)
        {
            using (var zip = ZipFile.OpenRead(path))
                return ReadSheetMap(zip).Select(s => s.Name).ToList();
        }

        /// <summary>
        /// Reads one sheet by name. Throws if the workbook has no such sheet, so the
        /// caller can report a clear message rather than silently importing nothing.
        /// </summary>
        public static XlsxSheet ReadSheet(string path, string sheetName)
        {
            using (var zip = ZipFile.OpenRead(path))
            {
                var map = ReadSheetMap(zip);
                var target = map.FirstOrDefault(s =>
                    string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase));
                if (target == null)
                    throw new InvalidDataException(
                        $"The workbook has no sheet named '{sheetName}'. " +
                        $"It has: {string.Join(", ", map.Select(s => s.Name))}.");

                var entry = zip.GetEntry(target.PartPath);
                if (entry == null)
                    throw new InvalidDataException($"Sheet part '{target.PartPath}' is missing from the file.");

                var shared = ReadSharedStrings(zip);
                var sheet = new XlsxSheet { Name = target.Name };

                XDocument doc;
                using (var stream = entry.Open())
                    doc = XDocument.Load(stream);

                foreach (var row in doc.Descendants(Main + "row"))
                {
                    if (!int.TryParse((string)row.Attribute("r"), out int rowNumber)) continue;

                    var cells = new Dictionary<int, string>();
                    foreach (var c in row.Elements(Main + "c"))
                    {
                        string reference = (string)c.Attribute("r");
                        if (string.IsNullOrEmpty(reference)) continue;
                        int col = ColumnIndexFromReference(reference);
                        if (col < 0) continue;

                        string text = CellText(c, shared);
                        if (string.IsNullOrEmpty(text)) continue; // treat blank as absent

                        cells[col] = text;
                        if (col > sheet.MaxColumnIndex) sheet.MaxColumnIndex = col;
                    }

                    if (cells.Count > 0) sheet.Rows[rowNumber] = cells;
                }

                return sheet;
            }
        }

        /// <summary>
        /// Best guess at which row holds the column headers. Needed because a real
        /// sheet rarely starts at row 1 — the file this was built for has its headers
        /// on row 3 of Sheet2 and row 2 of Sheet1, under a title/notes row.
        ///
        /// The rule: the first row near the top carrying at least three filled cells,
        /// of which at least two are not numbers. A title row ("Sample") has too few
        /// cells; a data row has mostly numbers. Returns the first populated row if
        /// nothing matches, so the caller always gets a usable starting point — the UI
        /// shows the result and lets it be corrected.
        /// </summary>
        public static int DetectHeaderRow(XlsxSheet sheet)
        {
            const int SearchDepth = 30;

            foreach (var kv in sheet.Rows)
            {
                if (kv.Key > SearchDepth) break;
                int filled = kv.Value.Count;
                int textual = kv.Value.Values.Count(v => !LooksNumeric(v));
                if (filled >= 3 && textual >= 2) return kv.Key;
            }

            return sheet.Rows.Count > 0 ? sheet.Rows.Keys.First() : 1;
        }

        /// <summary>0 -> "A", 25 -> "Z", 26 -> "AA".</summary>
        public static string ColumnName(int columnIndex)
        {
            if (columnIndex < 0) return "";
            string name = "";
            int n = columnIndex;
            while (true)
            {
                name = (char)('A' + (n % 26)) + name;
                n = n / 26 - 1;
                if (n < 0) break;
            }
            return name;
        }

        private static bool LooksNumeric(string value) =>
            double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

        private class SheetRef
        {
            public string Name;
            public string PartPath;
        }

        /// <summary>
        /// Maps each sheet name to its part path, via workbook.xml's r:id and the
        /// workbook relationships. The sheet order in workbook.xml is tab order; the
        /// part filenames do NOT reliably match it (sheet2.xml can be the first tab),
        /// so the relationship lookup is not optional.
        /// </summary>
        private static List<SheetRef> ReadSheetMap(ZipArchive zip)
        {
            var wbEntry = zip.GetEntry("xl/workbook.xml");
            if (wbEntry == null)
                throw new InvalidDataException("Not a valid .xlsx file: xl/workbook.xml is missing.");

            var rels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var relEntry = zip.GetEntry("xl/_rels/workbook.xml.rels");
            if (relEntry != null)
            {
                XDocument relDoc;
                using (var s = relEntry.Open()) relDoc = XDocument.Load(s);
                foreach (var r in relDoc.Descendants(PkgRel + "Relationship"))
                {
                    string id = (string)r.Attribute("Id");
                    string tgt = (string)r.Attribute("Target");
                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(tgt))
                        rels[id] = NormalizePart(tgt);
                }
            }

            XDocument wb;
            using (var s = wbEntry.Open()) wb = XDocument.Load(s);

            var list = new List<SheetRef>();
            foreach (var sheet in wb.Descendants(Main + "sheet"))
            {
                string name = (string)sheet.Attribute("name");
                string rid = (string)sheet.Attribute(Rel + "id");
                if (string.IsNullOrEmpty(name)) continue;

                string part = !string.IsNullOrEmpty(rid) && rels.TryGetValue(rid, out var p)
                    ? p
                    : null;
                if (part == null) continue;

                list.Add(new SheetRef { Name = name, PartPath = part });
            }
            return list;
        }

        /// <summary>
        /// Relationship targets come in several shapes — "worksheets/sheet2.xml"
        /// (relative to xl/), "/xl/worksheets/sheet2.xml" (absolute in the package).
        /// Both have to end up as the zip entry name.
        /// </summary>
        private static string NormalizePart(string target)
        {
            string t = target.Replace('\\', '/');
            if (t.StartsWith("/")) return t.TrimStart('/');
            if (t.StartsWith("xl/", StringComparison.OrdinalIgnoreCase)) return t;
            return "xl/" + t;
        }

        private static List<string> ReadSharedStrings(ZipArchive zip)
        {
            var result = new List<string>();
            var entry = zip.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return result; // a workbook of only numbers has no such part

            XDocument doc;
            using (var s = entry.Open()) doc = XDocument.Load(s);

            foreach (var si in doc.Descendants(Main + "si"))
                result.Add(SharedStringText(si));

            return result;
        }

        /// <summary>
        /// A shared string is either a plain &lt;t&gt; or a sequence of &lt;r&gt; runs,
        /// which must be concatenated. Text inside &lt;rPh&gt; is skipped: those are
        /// phonetic-guide runs that Excel adds for East Asian text, and including them
        /// would append a furigana/ruby copy of the string to itself.
        /// </summary>
        private static string SharedStringText(XElement si)
        {
            var parts = si.Descendants(Main + "t")
                .Where(t => !t.Ancestors(Main + "rPh").Any())
                .Select(t => t.Value);
            return string.Concat(parts);
        }

        private static string CellText(XElement c, List<string> shared)
        {
            string type = (string)c.Attribute("t") ?? "n";

            if (type == "inlineStr")
            {
                var isEl = c.Element(Main + "is");
                return isEl == null ? "" : SharedStringText(isEl);
            }

            var v = c.Element(Main + "v");
            if (v == null) return "";
            string raw = v.Value;

            switch (type)
            {
                case "s": // index into the shared string table
                    return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx)
                           && idx >= 0 && idx < shared.Count
                        ? shared[idx]
                        : "";

                case "str": // cached formula result, already text
                    return raw;

                case "b":
                    return raw == "1" ? "TRUE" : "FALSE";

                case "e": // error cell (#REF!, #N/A, ...) — pass it through visibly
                    return raw;

                default:
                    return FormatNumber(raw);
            }
        }

        /// <summary>
        /// Renders a stored number the way Excel shows it rather than the way it is
        /// stored. A sheet holding 10.03 stores 10.029999999999999, and 0.01415 stores
        /// 1.4149999999999999E-2; pasting either of those raw into the tool would be
        /// both ugly and wrong-looking next to the spreadsheet it came from. G15 —
        /// fifteen significant digits — is the precision Excel itself displays at, so
        /// it reproduces the on-screen value. Integers are printed without a decimal
        /// point so a pattern index reads as 1008, not 1008.0.
        /// </summary>
        private static string FormatNumber(string raw)
        {
            if (!double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                return raw;

            if (d == Math.Floor(d) && Math.Abs(d) < 1e15)
                return ((long)d).ToString(CultureInfo.InvariantCulture);

            return d.ToString("G15", CultureInfo.InvariantCulture);
        }

        /// <summary>"H4" -> 7. Returns -1 if the reference has no column letters.</summary>
        private static int ColumnIndexFromReference(string reference)
        {
            int n = 0, count = 0;
            foreach (char ch in reference)
            {
                char up = char.ToUpperInvariant(ch);
                if (up < 'A' || up > 'Z') break;
                n = n * 26 + (up - 'A' + 1);
                count++;
            }
            return count == 0 ? -1 : n - 1;
        }
    }
}
