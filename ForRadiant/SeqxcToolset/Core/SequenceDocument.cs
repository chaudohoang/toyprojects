using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SeqxcToolset.Core
{
    /// <summary>
    /// One entry from the &lt;PatternSetupList&gt;. Can either be a "terminal" entry
    /// (owns a real &lt;PatternNumber&gt; inside a nested Dove3p0-style &lt;Pattern xsi:type=...&gt;)
    /// or an "alias" entry whose &lt;Pattern&gt; just contains a &lt;PatternSetupName&gt; reference
    /// to another PatternSetup (e.g. r216 / g216 / b216 all pointing at W216).
    /// </summary>
    public class PatternSetupInfo
    {
        public string Name;
        public XElement Element;
        public XElement PatternWrap;
        public XElement TerminalPattern;
        public string AliasTarget;
        public string PatternNumberRaw;

        public bool IsAlias => TerminalPattern == null;
    }

    public class SequenceItemInfo
    {
        public int Index;
        public bool Selected;
        public string PatternSetupName;
        public string AnalysisType;
        public string UserName;
        public string Notes;

        // Live reference into the in-memory XDocument — editing these tags
        // directly mutates the document, same as PatternSetupInfo.Element.
        public XElement AnalysisElement;

        // Null when this item's Analysis type doesn't have these tags at all.
        public string LuminanceRed;
        public string LuminanceGreen;
        public string LuminanceBlue;
    }

    public class ChannelInfo
    {
        public int Index;          // 0-based position in the CaptureFilter/ExposureTime arrays
        public string Label;       // e.g. "Y (Green)"
        public bool Capture;
        public string ExposureMs;
    }

    public class PatternNumberChange
    {
        public string TerminalName;
        public string OldValue;
        public string NewValue;
    }

    /// <summary>
    /// A pending edit to one channel (Y/X/Z) of a PatternSetup's own
    /// CaptureFilter/ExposureTime arrays. Either or both of NewCapture/
    /// NewExposure may be set; null means "leave that field alone".
    /// </summary>
    public class ExposureChange
    {
        public string PatternSetupName;
        public int ChannelIndex;
        public string OldCapture;
        public string NewCapture;
        public string OldExposure;
        public string NewExposure;
    }

    /// <summary>
    /// A pending edit to one Luminance Scale field (Red/Green/Blue) on a
    /// SequenceItem's own Analysis element. Unlike PatternSetup entries,
    /// SequenceItems have no unique name, so changes are addressed by
    /// ordinal position (ItemIndex) instead.
    /// </summary>
    public class LuminanceScaleChange
    {
        public int ItemIndex;
        public string FieldTag; // "LuminanceScaleRed" / "LuminanceScaleGreen" / "LuminanceScaleBlue"
        public string OldValue;
        public string NewValue;
    }

    public class SequenceDocument
    {
        public static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

        public string FilePath { get; private set; }
        public XDocument Xml { get; private set; }

        public List<SequenceItemInfo> Items { get; } = new List<SequenceItemInfo>();
        public Dictionary<string, PatternSetupInfo> PatternSetups { get; } =
            new Dictionary<string, PatternSetupInfo>(StringComparer.OrdinalIgnoreCase);

        public void Load(string path)
        {
            FilePath = path;
            Xml = XDocument.Load(path);
            Items.Clear();
            PatternSetups.Clear();
            ParseItems();
            ParsePatternSetups();
        }

        private void ParseItems()
        {
            var itemsEl = Xml.Root?.Element("Items");
            if (itemsEl == null) return;

            int idx = 0;
            foreach (var item in itemsEl.Elements("SequenceItem"))
            {
                var analysis = item.Element("Analysis");
                string atype = analysis?.Attribute(Xsi + "type")?.Value;
                string selectedRaw = item.Element("Selected")?.Value ?? "false";

                Items.Add(new SequenceItemInfo
                {
                    Index = idx++,
                    Selected = selectedRaw.Trim().Equals("true", StringComparison.OrdinalIgnoreCase),
                    PatternSetupName = item.Element("PatternSetupName")?.Value,
                    AnalysisType = atype,
                    UserName = analysis?.Element("UserName")?.Value,
                    Notes = analysis?.Element("Notes")?.Value,
                    AnalysisElement = analysis,
                    LuminanceRed = analysis?.Element("LuminanceScaleRed")?.Value,
                    LuminanceGreen = analysis?.Element("LuminanceScaleGreen")?.Value,
                    LuminanceBlue = analysis?.Element("LuminanceScaleBlue")?.Value
                });
            }
        }

        private void ParsePatternSetups()
        {
            var listEl = Xml.Root?.Element("PatternSetupList");
            if (listEl == null) return;

            foreach (var ps in listEl.Elements("PatternSetup"))
            {
                string name = ps.Element("Name")?.Value;
                if (string.IsNullOrEmpty(name)) continue;
                if (PatternSetups.ContainsKey(name)) continue; // keep first occurrence

                var patternWrap = ps.Element("Pattern");
                var terminal = patternWrap?.Element("Pattern"); // nested element carrying xsi:type

                var info = new PatternSetupInfo
                {
                    Name = name,
                    Element = ps,
                    PatternWrap = patternWrap,
                    TerminalPattern = terminal
                };

                if (terminal != null)
                    info.PatternNumberRaw = terminal.Element("PatternNumber")?.Value;
                else
                    info.AliasTarget = patternWrap?.Element("PatternSetupName")?.Value;

                PatternSetups[name] = info;
            }
        }

        // (index, label) for the only 3 CaptureFilter/ExposureTime slots ever
        // populated in this file format — confirmed empirically: across all
        // 198 PatternSetups in a real file, indices 0/4/5/6 are always false;
        // only 1/2/3 (Y/X/Z tristimulus channels) are ever used.
        private static readonly (int Index, string Label)[] KnownChannels =
        {
            (1, "Y (Green)"),
            (2, "X (Red)"),
            (3, "Z (Blue)")
        };

        /// <summary>
        /// Reads CaptureFilter/ExposureTime directly off the named PatternSetup's
        /// own element — these live on the PatternSetup itself (unlike PatternNumber),
        /// so this does NOT follow the alias chain; each named entry has its own
        /// capture/exposure configuration regardless of pattern-number aliasing.
        /// </summary>
        public List<ChannelInfo> GetChannels(string patternSetupName)
        {
            var result = new List<ChannelInfo>();
            if (string.IsNullOrEmpty(patternSetupName) ||
                !PatternSetups.TryGetValue(patternSetupName, out var info))
                return result;

            var captureBools = info.Element.Element("CaptureFilter")?.Elements("boolean").ToList();
            var exposureFloats = info.Element.Element("ExposureTime")?.Elements("float").ToList();
            if (captureBools == null || exposureFloats == null) return result;

            foreach (var (idx, label) in KnownChannels)
            {
                if (idx >= captureBools.Count || idx >= exposureFloats.Count) continue;
                result.Add(new ChannelInfo
                {
                    Index = idx,
                    Label = label,
                    Capture = captureBools[idx].Value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase),
                    ExposureMs = exposureFloats[idx].Value.Trim()
                });
            }
            return result;
        }

        /// <summary>
        /// Follows the alias chain (r216 -> W216, etc.) until it finds the PatternSetup
        /// that actually owns a &lt;PatternNumber&gt;. Returns null if unresolved or circular.
        /// </summary>
        public PatternSetupInfo ResolveTerminal(string name, int depth = 0)
        {
            if (string.IsNullOrEmpty(name) || depth > 20) return null;
            if (!PatternSetups.TryGetValue(name, out var info)) return null;
            if (!info.IsAlias) return info;
            if (!string.IsNullOrEmpty(info.AliasTarget) &&
                !info.AliasTarget.Equals(name, StringComparison.OrdinalIgnoreCase))
                return ResolveTerminal(info.AliasTarget, depth + 1);
            return null;
        }

        public string GetResolvedPatternNumber(string patternSetupName)
        {
            return ResolveTerminal(patternSetupName)?.PatternNumberRaw;
        }

        /// <summary>
        /// True when this pattern's terminal IS referenced by at least one
        /// SequenceItem, but every item referencing it has Selected=false —
        /// i.e. it's a disabled/inactive step, not just an unused library
        /// entry. Used to keep disabled steps out of consideration for
        /// matching unless the person explicitly asks to see them.
        /// </summary>
        public bool IsPatternUsedOnlyByDisabledItems(string patternSetupName)
        {
            var terminal = ResolveTerminal(patternSetupName);
            if (terminal == null) return false;

            bool anyReference = false, anySelectedReference = false;
            foreach (var item in Items)
            {
                var t = ResolveTerminal(item.PatternSetupName);
                if (t == null || !t.Name.Equals(terminal.Name, StringComparison.OrdinalIgnoreCase)) continue;
                anyReference = true;
                if (item.Selected) anySelectedReference = true;
            }
            return anyReference && !anySelectedReference;
        }

        /// <summary>
        /// All other PatternSetup names (excluding terminalName itself) that resolve
        /// to the same terminal — i.e. siblings that would be affected by editing it.
        /// </summary>
        public List<string> GetSiblingAliases(string terminalName)
        {
            var result = new List<string>();
            foreach (var kv in PatternSetups)
            {
                if (kv.Key.Equals(terminalName, StringComparison.OrdinalIgnoreCase)) continue;
                var t = ResolveTerminal(kv.Key);
                if (t != null && t.Name.Equals(terminalName, StringComparison.OrdinalIgnoreCase))
                    result.Add(kv.Key);
            }
            return result;
        }

        /// <summary>
        /// Applies a new PatternNumber value in-memory (does not touch disk).
        /// Returns the resolved terminal name and any sibling aliases affected.
        /// </summary>
        public bool SetPatternNumber(string patternSetupName, string newValue,
            out string terminalName, out List<string> affectedSiblings)
        {
            terminalName = null;
            affectedSiblings = new List<string>();

            var terminal = ResolveTerminal(patternSetupName);
            if (terminal?.TerminalPattern == null) return false;

            var pnEl = terminal.TerminalPattern.Element("PatternNumber");
            if (pnEl == null) return false;

            terminalName = terminal.Name;
            affectedSiblings = GetSiblingAliases(terminal.Name);

            pnEl.Value = newValue;
            terminal.PatternNumberRaw = newValue;
            return true;
        }

        /// <summary>
        /// Writes changes to disk WITHOUT reformatting the whole file: it re-reads the
        /// original file as text, locates each changed PatternSetup block by &lt;Name&gt;,
        /// and replaces only the &lt;PatternNumber&gt; value inside that block. This keeps
        /// the diff minimal on a 70k+ line file, matching how the rest of the toolchain
        /// edits .seqxc files.
        /// </summary>
        public List<string> SaveMinimalDiff(string outputPath, IEnumerable<PatternNumberChange> changes)
        {
            var warnings = new List<string>();
            Encoding enc = DetectEncoding(FilePath);
            string text = File.ReadAllText(FilePath, enc);

            foreach (var change in changes)
            {
                bool ok = TryPatchPatternNumber(ref text, change.TerminalName, change.OldValue, change.NewValue);
                if (!ok)
                    warnings.Add($"Could not safely locate/patch '{change.TerminalName}' " +
                                 $"({change.OldValue} -> {change.NewValue}). Skipped to avoid corrupting the file.");
            }

            File.WriteAllText(outputPath, text, enc);
            return warnings;
        }

        /// <summary>
        /// Applies in-memory edits to one channel of a PatternSetup's OWN
        /// CaptureFilter/ExposureTime arrays (not alias-resolved — see
        /// GetChannels). Pass null for either newCapture/newExposure to leave
        /// that field untouched. Returns the old values for diff tracking.
        /// </summary>
        public bool SetChannelValue(string patternSetupName, int channelIndex,
            bool? newCapture, string newExposure, out string oldCapture, out string oldExposure)
        {
            oldCapture = null;
            oldExposure = null;

            if (!PatternSetups.TryGetValue(patternSetupName, out var info)) return false;

            var captureItems = info.Element.Element("CaptureFilter")?.Elements("boolean").ToList();
            var exposureItems = info.Element.Element("ExposureTime")?.Elements("float").ToList();
            if (captureItems == null || exposureItems == null) return false;
            if (channelIndex >= captureItems.Count || channelIndex >= exposureItems.Count) return false;

            if (newCapture.HasValue)
            {
                oldCapture = captureItems[channelIndex].Value;
                captureItems[channelIndex].Value = newCapture.Value ? "true" : "false";
            }
            if (newExposure != null)
            {
                oldExposure = exposureItems[channelIndex].Value;
                exposureItems[channelIndex].Value = newExposure;
            }
            return true;
        }

        /// <summary>
        /// Writes CaptureFilter/ExposureTime edits to disk with the same
        /// minimal-diff text patching as SaveMinimalDiff. Since these arrays
        /// have several indistinguishable &lt;boolean&gt;/&lt;float&gt; siblings,
        /// the patch counts to the Nth occurrence within the right container
        /// rather than matching on value text alone.
        /// </summary>
        public List<string> SaveExposureChanges(string outputPath, IEnumerable<ExposureChange> changes)
        {
            var warnings = new List<string>();
            Encoding enc = DetectEncoding(FilePath);
            string text = File.ReadAllText(FilePath, enc);

            foreach (var change in changes)
            {
                if (change.NewCapture != null)
                {
                    bool ok = TryPatchChannelValue(ref text, change.PatternSetupName, "CaptureFilter", "boolean",
                        change.ChannelIndex, change.OldCapture, change.NewCapture);
                    if (!ok)
                        warnings.Add($"Could not patch capture flag for '{change.PatternSetupName}' " +
                                     $"channel {change.ChannelIndex}. Skipped to avoid corrupting the file.");
                }
                if (change.NewExposure != null)
                {
                    bool ok = TryPatchChannelValue(ref text, change.PatternSetupName, "ExposureTime", "float",
                        change.ChannelIndex, change.OldExposure, change.NewExposure);
                    if (!ok)
                        warnings.Add($"Could not patch exposure time for '{change.PatternSetupName}' " +
                                     $"channel {change.ChannelIndex}. Skipped to avoid corrupting the file.");
                }
            }

            File.WriteAllText(outputPath, text, enc);
            return warnings;
        }

        private static bool TryPatchChannelValue(ref string text, string patternSetupName,
            string containerTag, string itemTag, int channelIndex, string oldValue, string newValue)
        {
            string nameTag = $"<Name>{patternSetupName}</Name>";
            int nameIdx = text.IndexOf(nameTag, StringComparison.Ordinal);
            if (nameIdx < 0) return false;

            int blockStart = text.LastIndexOf("<PatternSetup>", nameIdx, StringComparison.Ordinal);
            int blockEndTagIdx = text.IndexOf("</PatternSetup>", nameIdx, StringComparison.Ordinal);
            if (blockStart < 0 || blockEndTagIdx < 0) return false;
            int blockEnd = blockEndTagIdx + "</PatternSetup>".Length;
            string block = text.Substring(blockStart, blockEnd - blockStart);

            string openContainer = $"<{containerTag}>";
            string closeContainer = $"</{containerTag}>";
            int containerStart = block.IndexOf(openContainer, StringComparison.Ordinal);
            int containerEndIdx = block.IndexOf(closeContainer, StringComparison.Ordinal);
            if (containerStart < 0 || containerEndIdx < 0) return false;
            int containerEnd = containerEndIdx + closeContainer.Length;
            string container = block.Substring(containerStart, containerEnd - containerStart);

            string openItem = $"<{itemTag}>";
            string closeItem = $"</{itemTag}>";
            int searchFrom = 0, itemStart = -1, itemEnd = -1;
            for (int i = 0; i <= channelIndex; i++)
            {
                itemStart = container.IndexOf(openItem, searchFrom, StringComparison.Ordinal);
                if (itemStart < 0) return false;
                itemEnd = container.IndexOf(closeItem, itemStart, StringComparison.Ordinal);
                if (itemEnd < 0) return false;
                searchFrom = itemEnd + closeItem.Length;
            }

            string actualOld = container.Substring(itemStart + openItem.Length, itemEnd - (itemStart + openItem.Length));
            if (actualOld.Trim() != (oldValue ?? "").Trim()) return false; // safety: value drifted, don't guess

            string newItemFull = openItem + newValue + closeItem;
            string patchedContainer = container.Substring(0, itemStart) + newItemFull + container.Substring(itemEnd + closeItem.Length);
            string patchedBlock = block.Substring(0, containerStart) + patchedContainer + block.Substring(containerEnd);
            text = text.Substring(0, blockStart) + patchedBlock + text.Substring(blockEnd);
            return true;
        }

        private static Encoding DetectEncoding(string path)
        {
            using (var reader = new StreamReader(path, Encoding.UTF8, true))
            {
                reader.Peek();
                return reader.CurrentEncoding;
            }
        }

        private static bool TryPatchPatternNumber(ref string text, string terminalName, string oldValue, string newValue)
        {
            string nameTag = $"<Name>{terminalName}</Name>";
            int nameIdx = text.IndexOf(nameTag, StringComparison.Ordinal);
            if (nameIdx < 0) return false;

            int blockStart = text.LastIndexOf("<PatternSetup>", nameIdx, StringComparison.Ordinal);
            int blockEndTagIdx = text.IndexOf("</PatternSetup>", nameIdx, StringComparison.Ordinal);
            if (blockStart < 0 || blockEndTagIdx < 0) return false;
            int blockEnd = blockEndTagIdx + "</PatternSetup>".Length;

            string block = text.Substring(blockStart, blockEnd - blockStart);
            string oldTag = $"<PatternNumber>{oldValue}</PatternNumber>";
            string newTag = $"<PatternNumber>{newValue}</PatternNumber>";

            int tagIdx = block.IndexOf(oldTag, StringComparison.Ordinal);
            if (tagIdx < 0) return false;

            string patchedBlock = block.Substring(0, tagIdx) + newTag + block.Substring(tagIdx + oldTag.Length);
            text = text.Substring(0, blockStart) + patchedBlock + text.Substring(blockEnd);
            return true;
        }

        /// <summary>
        /// Applies an in-memory edit to one field on a SequenceItem's own
        /// Analysis element (e.g. LuminanceScaleRed) by ordinal item index —
        /// SequenceItems have no unique name to key off, unlike PatternSetups.
        /// </summary>
        public bool SetSequenceItemField(int itemIndex, string fieldTag, string newValue, out string oldValue)
        {
            oldValue = null;
            if (itemIndex < 0 || itemIndex >= Items.Count) return false;

            var el = Items[itemIndex].AnalysisElement?.Element(fieldTag);
            if (el == null) return false;

            oldValue = el.Value;
            el.Value = newValue;
            return true;
        }

        /// <summary>
        /// Writes Luminance Scale edits to disk with the same minimal-diff
        /// text patching used elsewhere, but locates the target block by
        /// counting to the Nth &lt;SequenceItem&gt; occurrence (0-based) rather
        /// than a &lt;Name&gt; match, since SequenceItems have none.
        /// </summary>
        public List<string> SaveLuminanceScaleChanges(string outputPath, IEnumerable<LuminanceScaleChange> changes)
        {
            var warnings = new List<string>();
            Encoding enc = DetectEncoding(FilePath);
            string text = File.ReadAllText(FilePath, enc);

            foreach (var change in changes)
            {
                bool ok = TryPatchNthSequenceItemField(ref text, change.ItemIndex, change.FieldTag,
                    change.OldValue, change.NewValue);
                if (!ok)
                    warnings.Add($"Could not patch {change.FieldTag} for step #{change.ItemIndex + 1}. " +
                                 "Skipped to avoid corrupting the file.");
            }

            File.WriteAllText(outputPath, text, enc);
            return warnings;
        }

        private static bool TryPatchNthSequenceItemField(ref string text, int itemIndex, string fieldTag,
            string oldValue, string newValue)
        {
            const string openItem = "<SequenceItem>";
            const string closeItem = "</SequenceItem>";

            int searchFrom = 0, blockStart = -1, blockEnd = -1;
            for (int i = 0; i <= itemIndex; i++)
            {
                blockStart = text.IndexOf(openItem, searchFrom, StringComparison.Ordinal);
                if (blockStart < 0) return false;
                int endIdx = text.IndexOf(closeItem, blockStart, StringComparison.Ordinal);
                if (endIdx < 0) return false;
                blockEnd = endIdx + closeItem.Length;
                searchFrom = blockEnd;
            }

            string block = text.Substring(blockStart, blockEnd - blockStart);
            string oldTag = $"<{fieldTag}>{oldValue}</{fieldTag}>";
            string newTag = $"<{fieldTag}>{newValue}</{fieldTag}>";

            int tagIdx = block.IndexOf(oldTag, StringComparison.Ordinal);
            if (tagIdx < 0) return false;

            string patchedBlock = block.Substring(0, tagIdx) + newTag + block.Substring(tagIdx + oldTag.Length);
            text = text.Substring(0, blockStart) + patchedBlock + text.Substring(blockEnd);
            return true;
        }
    }
}
