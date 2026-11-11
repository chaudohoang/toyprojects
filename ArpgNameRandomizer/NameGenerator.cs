using System;
using System.Collections.Generic;
using System.Linq;

namespace ArpgNameRandomizer
{
    enum Gender { Any, Male, Female }
    enum Style { Mixed, Fantasy, Compound, NameAndTitle }

    /// <summary>Every name a category's current settings can make; hands out ones not in <c>seen</c>.</summary>
    interface INamePool
    {
        long Total(Style s);
        List<string> Take(int count, Style style, ISet<string> seen, Random rnd);
    }

    /// <summary>
    /// Every name the current settings can produce, listed up front rather than rolled at random,
    /// so the app can hand out names that were never shown before and say exactly how many are left.
    ///
    /// Made-up and compound names are small enough to hold as lists. Name + title (first name x title)
    /// runs to hundreds of thousands, so it is counted, and sampled on demand, without building the list.
    /// </summary>
    class NamePool : INamePool
    {
        public readonly List<string> Fantasy = new List<string>();
        public readonly List<string> Compound = new List<string>();
        readonly HashSet<string> fantasySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        readonly HashSet<string> compoundSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Name + title: each first name may pair with the titles of every theme it belongs to.
        readonly Dictionary<string, string[]> titlesFor = new Dictionary<string, string[]>(StringComparer.Ordinal);
        readonly List<string> firsts = new List<string>();
        readonly int minLen, maxLen;
        public readonly long TitledTotal;

        const int MaxFirstLen = 8;

        public NamePool(GameClass cls, Gender gender, int maxLen, string prefix)
        {
            prefix = (prefix ?? "").Trim();
            minLen = 3;
            this.maxLen = maxLen;

            var firstThemes = new Dictionary<string, HashSet<Theme>>(StringComparer.Ordinal);
            var themeTitles = new Dictionary<Theme, List<string>>();

            foreach (var t in cls.Themes)
            {
                foreach (var name in FantasyNames(t, gender, prefix))
                {
                    if (name.Length >= minLen && name.Length <= maxLen && fantasySet.Add(name)) Fantasy.Add(name);
                    if (name.Length <= MaxFirstLen)
                    {
                        if (!firstThemes.TryGetValue(name, out var set)) firstThemes[name] = set = new HashSet<Theme>();
                        set.Add(t);
                    }
                }
                var titles = new List<string>();
                foreach (var c in CompoundNames(t))
                {
                    titles.Add(c);
                    bool fitsPrefix = prefix.Length == 0 || c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                    if (fitsPrefix && c.Length >= minLen && c.Length <= maxLen && !fantasySet.Contains(c) && compoundSet.Add(c))
                        Compound.Add(c);
                }
                themeTitles[t] = titles;
            }
            // A later theme's made-up name can equal an earlier theme's compound; count it once, as made-up.
            Compound.RemoveAll(c => fantasySet.Contains(c));
            compoundSet.ExceptWith(fantasySet);

            // Firsts that share the same set of themes share one merged title list.
            var merged = new Dictionary<string, string[]>();
            foreach (var kv in firstThemes)
            {
                string key = string.Join("|", kv.Value.Select(t => t.Name).OrderBy(n => n));
                if (!merged.TryGetValue(key, out var list))
                    merged[key] = list = kv.Value.SelectMany(t => themeTitles[t]).Distinct(StringComparer.Ordinal).ToArray();
                int fits = list.Count(c => Fits(kv.Key.Length + c.Length));
                if (fits == 0) continue;
                titlesFor[kv.Key] = list;
                firsts.Add(kv.Key);
                TitledTotal += fits;
            }
        }

        bool Fits(int len) => len >= minLen && len <= maxLen;

        public long Total(Style s) =>
            s == Style.Fantasy ? Fantasy.Count :
            s == Style.Compound ? Compound.Count :
            s == Style.NameAndTitle ? TitledTotal :
            Fantasy.Count + Compound.Count + TitledTotal;

        public long Remaining(Style s, ISet<string> seen)
        {
            long usedF = 0, usedC = 0, usedT = 0;
            foreach (var n in seen)
            {
                if (fantasySet.Contains(n)) usedF++;
                else if (compoundSet.Contains(n)) usedC++;
                else if (IsTitled(n)) usedT++;
            }
            return s == Style.Fantasy ? Fantasy.Count - usedF :
                   s == Style.Compound ? Compound.Count - usedC :
                   s == Style.NameAndTitle ? TitledTotal - usedT :
                   Fantasy.Count + Compound.Count + TitledTotal - usedF - usedC - usedT;
        }

        // "KaelDuskborn" -> "Kael" + "Duskborn": a title name has exactly one capital after the first letter.
        bool IsTitled(string name)
        {
            int split = -1;
            for (int i = 1; i < name.Length; i++)
                if (char.IsUpper(name[i])) { if (split >= 0) return false; split = i; }
            if (split < 0 || !Fits(name.Length)) return false;
            return titlesFor.TryGetValue(name.Substring(0, split), out var titles)
                && Array.IndexOf(titles, name.Substring(split)) >= 0;
        }

        /// <summary>Up to <paramref name="count"/> names not in <paramref name="seen"/>; each is added to it.</summary>
        public List<string> Take(int count, Style style, ISet<string> seen, Random rnd)
        {
            var result = new List<string>();
            var fQueue = new Queue<string>(Shuffle(Fantasy.Where(n => !seen.Contains(n)), rnd));
            var cQueue = new Queue<string>(Shuffle(Compound.Where(n => !seen.Contains(n)), rnd));
            long tLeft = style == Style.Mixed || style == Style.NameAndTitle ? Remaining(Style.NameAndTitle, seen) : 0;

            while (result.Count < count)
            {
                bool fOk = (style == Style.Mixed || style == Style.Fantasy) && fQueue.Count > 0;
                bool cOk = (style == Style.Mixed || style == Style.Compound) && cQueue.Count > 0;
                bool tOk = tLeft > 0;
                if (!fOk && !cOk && !tOk) break;

                // Mixed: roughly 45% made-up, 35% compound, 20% name + title, skipping any that ran out.
                double wF = fOk ? 45 : 0, wC = cOk ? 35 : 0, wT = tOk ? 20 : 0;
                double r = rnd.NextDouble() * (wF + wC + wT);
                string name;
                if (r < wF) name = fQueue.Dequeue();
                else if (r < wF + wC) name = cQueue.Dequeue();
                else
                {
                    name = NextTitled(seen, rnd);
                    if (name == null) { tLeft = 0; continue; }
                    tLeft--;
                }
                seen.Add(name);
                result.Add(name);
            }
            return result;
        }

        string NextTitled(ISet<string> seen, Random rnd)
        {
            // Random picks almost always hit an unseen name; only near the end of the pool do we walk it.
            for (int i = 0; i < 2000 && firsts.Count > 0; i++)
            {
                string f = firsts[rnd.Next(firsts.Count)];
                var titles = titlesFor[f];
                string c = titles[rnd.Next(titles.Length)];
                string name = f + c;
                if (Fits(name.Length) && !seen.Contains(name)) return name;
            }
            foreach (var f in Shuffle(firsts, rnd))
                foreach (var c in titlesFor[f])
                {
                    string name = f + c;
                    if (Fits(name.Length) && !seen.Contains(name)) return name;
                }
            return null;
        }

        static List<string> Shuffle(IEnumerable<string> items, Random rnd)
        {
            var list = items.ToList();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
            return list;
        }

        // ---- building blocks ----

        // "Mor" + ("ga") + "oth" -> Morgoth-ish made-up names. With a prefix, it also serves as an opener.
        static IEnumerable<string> FantasyNames(Theme t, Gender g, string prefix)
        {
            var starts = t.Starts.AsEnumerable();
            if (prefix.Length > 0) starts = starts.Concat(new[] { Cap(prefix.ToLowerInvariant()) });
            var ends = g == Gender.Male ? t.EndsM : g == Gender.Female ? t.EndsF : t.EndsM.Concat(t.EndsF).ToArray();
            var mids = new[] { "" }.Concat(t.Mids).ToArray();

            foreach (var s in starts)
                foreach (var m in mids)
                    foreach (var e in ends)
                    {
                        string name = m.Length > 0 ? Join(s, m) : s;
                        name = Cap(Join(name, e));
                        if (prefix.Length > 0 && !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                        if (Pronounceable(name)) yield return name;
                    }
        }

        // "Bone" + "caller" -> Bonecaller
        static IEnumerable<string> CompoundNames(Theme t)
        {
            foreach (var a in t.WordA)
                foreach (var bRaw in t.WordB)
                {
                    string b = bRaw.ToLowerInvariant();
                    if (char.ToLowerInvariant(a[a.Length - 1]) == b[0]) continue;                       // Steellash
                    if (a.Equals(b, StringComparison.OrdinalIgnoreCase)) continue;
                    if (b.Length >= 2 && a.EndsWith(b.Substring(0, 2), StringComparison.OrdinalIgnoreCase)) continue; // Ashshroud
                    yield return a + b;
                }
        }

        static string Join(string a, string b)
        {
            b = b.ToLowerInvariant();
            char la = char.ToLowerInvariant(a[a.Length - 1]);
            char fb = b[0];
            if (IsVowel(la) && IsVowel(fb)) b = b.Substring(1);          // Thra+ana -> Thrana
            else if (la == fb && !IsVowel(la)) b = b.Substring(1);       // Gor+rok -> Gorok
            return b.Length == 0 ? a : a + b;
        }

        static readonly string[] Digraphs = { "th", "sh", "ch", "ph", "gh", "ck", "wh" };

        // Reject things like "Aeia", "Kragthr", "Xxx".
        static bool Pronounceable(string name)
        {
            string s = name.ToLowerInvariant();
            foreach (var d in Digraphs) s = s.Replace(d, "T");   // treat as one consonant
            int vowels = 0, cons = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (i >= 2 && c == s[i - 1] && c == s[i - 2]) return false;
                if (IsVowel(c)) { vowels++; cons = 0; if (vowels > 2) return false; }
                else { cons++; vowels = 0; if (cons > 2) return false; }
            }
            // Two vowels in a row is fine ("ae", "ia"), but not the same one twice ("aa", "ee", "oo").
            for (int i = 1; i < s.Length; i++)
                if (s[i] == s[i - 1] && IsVowel(s[i])) return false;
            // A letter pair that shows up twice reads as a stutter: Uraraji, Sererira, Brugrigorn.
            string low = name.ToLowerInvariant();
            var pairs = new HashSet<string>();
            for (int i = 1; i < low.Length; i++)
                if (!pairs.Add(low.Substring(i - 1, 2))) return false;
            return true;
        }

        static bool IsVowel(char c) => "aeiouy".IndexOf(char.ToLowerInvariant(c)) >= 0;

        static string Cap(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
