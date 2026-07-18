using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SeqxcEditor {

    // ── ClipItem — one property row copied to the shared clip list ─────────────
    class ClipItem {
        public PropRow Row;
        public string  Source;       // "analysis" or "patternSetup"
        public string  AnalysisType; // xsi:type of the source step (for type matching)
        public string  StepName;     // for display
    }

    // ── StepInfo ──────────────────────────────────────────────────────────────
    class StepInfo {
        public XmlNode Si;              // <SequenceItem> XML node
        public XmlNode An;              // analysis child node (e.g. DemuraLGDPOCB4p1_Net)
        public XmlNode Psn;             // linked <PatternSetup> node
        public string Guid;
        public string Name;             // from UserName in analysis, else PatternSetupName
        public string PatternSetupName;
        public string AnalysisTypeName; // XML tag name of the analysis node
        public string Badge;            // short badge: "POCB", "REG", etc.
        public bool Enabled;            // from <Selected>
    }

    // ── PropRow ───────────────────────────────────────────────────────────────
    class PropRow {
        public string Tag;          // XML element local name (may have [n] suffix for repeated tags)
        public string DisplayName;  // what to show in the grid (full path when search result)
        public string FullPath;     // e.g. "CameraSettingsList › CameraSettings[0] › ColorCalID"
        public string Value;        // text content or summary
        public XmlNode El;          // the actual XML element
        public bool IsComplex;      // true if element has child elements (array/object)
        public int Indent;          // nesting depth for display indentation
        public bool IsSearchResult; // came from filter/search — DisplayName is full path

        // ── Public factories ──────────────────────────────────────────────────

        /// Build top-level rows from a node; if filter given, do deep search instead.
        public static List<PropRow> FromNode(XmlNode node, string filter = "") {
            if (node == null) return new List<PropRow>();
            string f = (filter ?? "").Trim().ToLower();
            if (f.Length > 0) {
                return CollectLeaves(node, "")
                    .Where(r => r.Tag.ToLower().Contains(f) ||
                                r.Value.ToLower().Contains(f) ||
                                r.FullPath.ToLower().Contains(f))
                    .ToList();
            }
            return BuildLevel(node, 0, "");
        }

        /// Get child rows for an expanded complex row.
        public static List<PropRow> GetChildren(PropRow parent) =>
            BuildLevel(parent.El, parent.Indent + 1, parent.FullPath);

        // ── Private helpers ───────────────────────────────────────────────────

        static List<PropRow> BuildLevel(XmlNode node, int indent, string pathPrefix) {
            var rows = new List<PropRow>();
            var tc = TagCounts(node);
            var ti = new Dictionary<string, int>();
            foreach (XmlNode c in node.ChildNodes) {
                if (c.NodeType != XmlNodeType.Element) continue;
                var tag = c.LocalName;
                int idx = ti.ContainsKey(tag) ? ti[tag] : 0;
                ti[tag] = idx + 1;
                string dt = tc[tag] > 1 ? $"{tag}[{idx}]" : tag;
                string fp = pathPrefix.Length > 0 ? $"{pathPrefix} › {dt}" : dt;
                bool cx = c.ChildNodes.OfType<XmlElement>().Any();
                int fc = cx ? c.ChildNodes.OfType<XmlElement>().Count() : 0;
                rows.Add(new PropRow {
                    Tag         = tag,
                    DisplayName = dt,
                    FullPath    = fp,
                    Indent      = indent,
                    Value       = cx ? $"({fc} fields)" : c.InnerText.Trim(),
                    El          = c,
                    IsComplex   = cx,
                });
            }
            return rows;
        }

        static List<PropRow> CollectLeaves(XmlNode node, string prefix) {
            var rows = new List<PropRow>();
            var tc = TagCounts(node);
            var ti = new Dictionary<string, int>();
            foreach (XmlNode c in node.ChildNodes) {
                if (c.NodeType != XmlNodeType.Element) continue;
                var tag = c.LocalName;
                int idx = ti.ContainsKey(tag) ? ti[tag] : 0;
                ti[tag] = idx + 1;
                string dt = tc[tag] > 1 ? $"{tag}[{idx}]" : tag;
                string fp = prefix.Length > 0 ? $"{prefix} › {dt}" : dt;
                if (c.ChildNodes.OfType<XmlElement>().Any())
                    rows.AddRange(CollectLeaves(c, fp));
                else
                    rows.Add(new PropRow {
                        Tag = dt, DisplayName = fp, FullPath = fp,
                        Value = c.InnerText.Trim(), El = c,
                        IsComplex = false, IsSearchResult = true, Indent = 0,
                    });
            }
            return rows;
        }

        static Dictionary<string, int> TagCounts(XmlNode node) {
            var d = new Dictionary<string, int>();
            foreach (XmlNode c in node.ChildNodes)
                if (c.NodeType == XmlNodeType.Element)
                    d[c.LocalName] = d.ContainsKey(c.LocalName) ? d[c.LocalName] + 1 : 1;
            return d;
        }
    }

    // ── SeqxcDoc ──────────────────────────────────────────────────────────────
    static class SeqxcDoc {
        public static XmlDocument Doc;
        public static string FilePath = "";
        public static List<StepInfo> Steps = new List<StepInfo>();
        public static Dictionary<string, XmlNode> PatternSetups =
            new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);

        public static bool IsLoaded => Doc != null;

        // ── Load / Save ───────────────────────────────────────────────────────

        public static void Load(string path) {
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.Load(path);
            Doc = doc;
            FilePath = path;
            Reparse();
        }

        public static void Save(string path) {
            Doc.Save(path);
            FilePath = path;
        }

        // ── Parse ─────────────────────────────────────────────────────────────

        public static void Reparse() {
            Steps.Clear();
            PatternSetups.Clear();

            // PatternSetups
            var psl = Doc.SelectSingleNode("//PatternSetupList");
            if (psl != null)
                foreach (XmlNode ps in psl.ChildNodes) {
                    if (ps.NodeType != XmlNodeType.Element) continue;
                    var n = ps.SelectSingleNode("Name")?.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(n)) PatternSetups[n] = ps;
                }

            // SequenceItems
            var items = Doc.SelectNodes("//Items/SequenceItem");
            if (items == null) return;
            foreach (XmlNode si in items) {
                var s = new StepInfo { Si = si };
                s.Guid    = si.SelectSingleNode("GUID")?.InnerText?.Trim() ?? Guid.NewGuid().ToString();
                s.Enabled = si.SelectSingleNode("Selected")?.InnerText?.Trim()?.ToLower() != "false";
                s.PatternSetupName = si.SelectSingleNode("PatternSetupName")?.InnerText?.Trim() ?? "";

                foreach (XmlNode c in si.ChildNodes) {
                    if (c.NodeType != XmlNodeType.Element) continue;
                    if (c.LocalName == "Selected" || c.LocalName == "PatternSetupName" || c.LocalName == "GUID") continue;
                    s.An = c;
                    // TrueTest serializes analysis as <Analysis xsi:type="FullTypeName">
                    // Read the xsi:type attribute to get the real type name for the badge
                    string typeName = c.LocalName;
                    foreach (XmlAttribute attr in c.Attributes)
                        if (attr.LocalName == "type") { typeName = attr.Value; break; }
                    s.AnalysisTypeName = typeName;
                    s.Badge = BadgeFor(typeName);
                    var un = c.SelectSingleNode("UserName")?.InnerText?.Trim();
                    s.Name  = !string.IsNullOrWhiteSpace(un) ? un : s.PatternSetupName;
                    break;
                }
                if (string.IsNullOrEmpty(s.Name)) s.Name = s.PatternSetupName;
                if (!string.IsNullOrEmpty(s.PatternSetupName) && PatternSetups.ContainsKey(s.PatternSetupName))
                    s.Psn = PatternSetups[s.PatternSetupName];
                Steps.Add(s);
            }
        }

        static string BadgeFor(string typeName) {
            if (typeName.Contains("Register")) return "REG";
            if (typeName.Contains("POCB"))     return "POCB";
            if (typeName.Contains("MeasOnly") || typeName.Contains("MeasurementOnly")) return "MEAS";
            return typeName.Length >= 4 ? typeName.Substring(0, 4).ToUpper() : typeName.ToUpper();
        }

        // Human-readable short label for display in the step list
        public static string TypeLabelFor(string typeName) {
            if (string.IsNullOrEmpty(typeName)) return "";
            // Strip namespace prefix (everything up to last dot)
            int dot = typeName.LastIndexOf('.');
            string name = dot >= 0 ? typeName.Substring(dot + 1) : typeName;
            // Make it readable: split on capitals
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++) {
                if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i-1]))
                    sb.Append(' ');
                sb.Append(name[i]);
            }
            // Trim common suffixes
            string result = sb.ToString()
                .Replace("  ", " ")
                .Trim();
            return result;
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public static int     SharedCount(string psName) =>
            string.IsNullOrEmpty(psName) ? 0 :
            Steps.Count(s => string.Equals(s.PatternSetupName, psName, StringComparison.OrdinalIgnoreCase));

        public static StepInfo GetByGuid(string guid) =>
            Steps.FirstOrDefault(s => s.Guid == guid);

        // ── Paste ─────────────────────────────────────────────────────────────

        /// Paste a copied PropRow into targetNode with smart CameraSettings handling.
        public static void PasteRow(PropRow clip, XmlNode targetNode) {
            if (clip == null || targetNode == null) return;

            bool isCSLeaf  = !clip.IsComplex && clip.FullPath.Contains("CameraSettings");
            bool isCSBlock = clip.IsComplex   && clip.El.LocalName == "CameraSettings";
            bool isCSList  = clip.IsComplex   && clip.El.LocalName == "CameraSettingsList";

            if (isCSLeaf) {
                // Broadcast leaf value to ALL CameraSettings blocks
                var csl = targetNode.SelectSingleNode("CameraSettingsList");
                if (csl != null)
                    foreach (XmlNode cs in csl.ChildNodes) {
                        if (cs.NodeType != XmlNodeType.Element) continue;
                        var t = cs.SelectSingleNode(clip.Tag);
                        if (t != null) t.InnerText = clip.Value;
                    }
            } else if (isCSBlock) {
                // Copy cal fields (skip SerialNumber) to ALL CameraSettings blocks
                var csl = targetNode.SelectSingleNode("CameraSettingsList");
                if (csl != null)
                    foreach (XmlNode cs in csl.ChildNodes) {
                        if (cs.NodeType != XmlNodeType.Element) continue;
                        foreach (XmlNode f in clip.El.ChildNodes) {
                            if (f.NodeType != XmlNodeType.Element) continue;
                            if (f.LocalName == "SerialNumber") continue;
                            var t = cs.SelectSingleNode(f.LocalName);
                            if (t != null) t.InnerText = f.InnerText;
                        }
                    }
            } else if (isCSList) {
                // Smart merge: use first entry as cal template, apply to all target blocks
                var tCsl  = targetNode.SelectSingleNode("CameraSettingsList");
                var srcCs = clip.El.ChildNodes.OfType<XmlElement>().FirstOrDefault();
                if (tCsl != null && srcCs != null)
                    foreach (XmlNode cs in tCsl.ChildNodes) {
                        if (cs.NodeType != XmlNodeType.Element) continue;
                        foreach (XmlNode f in srcCs.ChildNodes) {
                            if (f.NodeType != XmlNodeType.Element) continue;
                            if (f.LocalName == "SerialNumber") continue;
                            var t = cs.SelectSingleNode(f.LocalName);
                            if (t != null) t.InnerText = f.InnerText;
                        }
                    }
            } else if (clip.IsComplex) {
                // Replace or append complex node
                var existing = targetNode.SelectSingleNode(clip.El.LocalName);
                var clone    = Doc.ImportNode(clip.El, true);
                if (existing != null) targetNode.ReplaceChild(clone, existing);
                else                  targetNode.AppendChild(clone);
            } else {
                // Simple leaf value
                var t = targetNode.SelectSingleNode(clip.Tag);
                if (t != null) t.InnerText = clip.Value;
            }
        }

        // ── Clone steps ───────────────────────────────────────────────────────

        public static List<string> CloneSteps(IEnumerable<string> guids) {
            var cloned = new List<string>();
            var psl = Doc.SelectSingleNode("//PatternSetupList");
            foreach (var guid in guids) {
                var step = GetByGuid(guid);
                if (step == null) continue;
                string newName = NextName(step.PatternSetupName);

                // Clone PatternSetup
                if (step.Psn != null && psl != null) {
                    var newPs = step.Psn.CloneNode(true);
                    SetText(newPs, "Name", newName);
                    var innerPsn = newPs.SelectSingleNode("Pattern/Pattern/PatternSetupName");
                    if (innerPsn != null) innerPsn.InnerText = newName;
                    psl.AppendChild(newPs);
                }

                // Clone SequenceItem
                var newSi = step.Si.CloneNode(true);
                SetText(newSi, "GUID", Guid.NewGuid().ToString());
                SetText(newSi, "PatternSetupName", newName);
                foreach (XmlNode c in newSi.ChildNodes) {
                    if (c.NodeType != XmlNodeType.Element) continue;
                    if (c.LocalName == "Selected" || c.LocalName == "PatternSetupName" || c.LocalName == "GUID") continue;
                    var un = c.SelectSingleNode("UserName");
                    if (un != null) un.InnerText = newName;
                    break;
                }
                step.Si.ParentNode?.InsertAfter(newSi, step.Si);
                cloned.Add(newName);
            }
            Reparse();
            return cloned;
        }

        // ── Delete steps ──────────────────────────────────────────────────────

        public static void DeleteSteps(IEnumerable<string> guids) {
            var psl   = Doc.SelectSingleNode("//PatternSetupList");
            var toDelete = guids.ToHashSet();
            foreach (var guid in toDelete) {
                var step = GetByGuid(guid);
                if (step == null) continue;
                step.Si.ParentNode?.RemoveChild(step.Si);
                // Remove PatternSetup if not used by any other remaining step
                if (!string.IsNullOrEmpty(step.PatternSetupName) &&
                    Steps.Count(s => !toDelete.Contains(s.Guid) &&
                                     string.Equals(s.PatternSetupName, step.PatternSetupName,
                                                   StringComparison.OrdinalIgnoreCase)) == 0) {
                    if (step.Psn != null && psl != null) psl.RemoveChild(step.Psn);
                }
            }
            Reparse();
        }

        // ── Move step ─────────────────────────────────────────────────────────

        public static void MoveStep(string guid, int direction) {
            var step = GetByGuid(guid);
            if (step == null) return;
            var parent   = step.Si.ParentNode;
            if (parent == null) return;
            var siblings = parent.ChildNodes.OfType<XmlElement>().ToList();
            int idx      = siblings.IndexOf(step.Si as XmlElement);
            int newIdx   = idx + direction;
            if (newIdx < 0 || newIdx >= siblings.Count) return;
            if (direction < 0) parent.InsertBefore(step.Si, siblings[newIdx]);
            else               parent.InsertAfter(step.Si,  siblings[newIdx]);
            Reparse();
        }

        // ── Toggle enabled ────────────────────────────────────────────────────

        public static void ToggleEnabled(string guid) {
            var step = GetByGuid(guid);
            if (step == null) return;
            step.Enabled = !step.Enabled;
            var sel = step.Si.SelectSingleNode("Selected");
            if (sel != null) sel.InnerText = step.Enabled.ToString().ToLower();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        static string NextName(string baseName) {
            string root = Regex.Replace(baseName ?? "", @"r\d*$", "");
            var existing = new HashSet<string>(PatternSetups.Keys, StringComparer.OrdinalIgnoreCase);
            string candidate = root + "r";
            for (int n = 2; existing.Contains(candidate); n++)
                candidate = root + "r" + n;
            return candidate;
        }

        /// Paste all clip items to a target step.
        /// Analysis items only paste if the target step's AnalysisType matches exactly.
        /// PatternSetup items always paste (with smart CameraSettings handling).
        public static void PasteClipItems(List<ClipItem> items, StepInfo target) {
            foreach (var clip in items) {
                if (clip.Source == "analysis") {
                    if (!string.Equals(target.AnalysisTypeName, clip.AnalysisType, StringComparison.Ordinal))
                        continue; // skip — analysis type mismatch
                    if (target.An != null) PasteRow(clip.Row, target.An);
                } else {
                    if (target.Psn != null) PasteRow(clip.Row, target.Psn);
                }
            }
        }

        static void SetText(XmlNode node, string tag, string value) {
            var c = node.SelectSingleNode(tag);
            if (c != null) c.InnerText = value;
        }
    }
}
