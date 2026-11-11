using System;
using System.Collections.Generic;
using System.Linq;

namespace NameRandomizer
{
    enum Gender { Any, Male, Female }
    enum Style { Mixed, Fantasy, Compound, NameAndTitle }

    /// <summary>Every name a category's current settings can make; hands out ones not in <c>seen</c>.</summary>
    interface INamePool
    {
        long Total(Style s);
        List<string> Take(int count, Style style, ISet<string> seen, Random rnd);
        /// <summary>Any name from the pool (seen or not), or null if the pool is empty.</summary>
        string RandomName(Style s, Random rnd);
    }

    /// <summary>
    /// Adds two digits to another pool's names (Grimjaw07, TacoGoblin42): 100 versions of every name.
    /// The inner pool is built two characters shorter so the number still fits the max length.
    /// </summary>
    class NumberedPool : INamePool
    {
        readonly INamePool inner;
        public NumberedPool(INamePool inner) { this.inner = inner; }

        public long Total(Style s) => inner.Total(s) * 100;

        public string RandomName(Style s, Random rnd)
        {
            string b = inner.RandomName(s, rnd);
            return b == null ? null : b + rnd.Next(100).ToString("00");
        }

        public List<string> Take(int count, Style style, ISet<string> seen, Random rnd)
        {
            var result = new List<string>();
            // With 100x the names, random picks practically never collide; give up only on a tiny,
            // nearly used-up pool (the caller then starts the cycle over).
            for (int miss = 0; result.Count < count && miss < 5000; )
            {
                string n = RandomName(style, rnd);
                if (n == null) break;
                if (seen.Contains(n)) { miss++; continue; }
                seen.Add(n);
                result.Add(n);
            }
            return result;
        }
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

        /// <summary>
        /// Everything that does not depend on max length, built once per class / gender / prefix and cached,
        /// so stepping the max length only re-filters (building "Any class" from scratch takes ~0.5 s).
        /// </summary>
        class Base
        {
            public List<string> Fantasy = new List<string>();
            public List<string> Compound = new List<string>();
            public List<(string first, string[] titles, int[] upTo)> Firsts = new List<(string, string[], int[])>();
        }

        static readonly Dictionary<string, Base> baseCache = new Dictionary<string, Base>();

        static Base GetBase(GameClass cls, Gender gender, string prefix)
        {
            string key = cls.Name + "|" + gender + "|" + prefix.ToLowerInvariant();
            if (baseCache.TryGetValue(key, out var b)) return b;
            b = new Base();

            var fantasySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var compoundSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var firstThemes = new Dictionary<string, HashSet<Theme>>(StringComparer.Ordinal);
            var themeTitles = new Dictionary<Theme, List<string>>();

            foreach (var t in cls.Themes)
            {
                foreach (var name in FantasyNames(t, gender, prefix))
                {
                    if (fantasySet.Add(name)) b.Fantasy.Add(name);
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
                    if (fitsPrefix && compoundSet.Add(c)) b.Compound.Add(c);
                }
                themeTitles[t] = titles;
            }
            // A made-up name can equal a compound; count it once, as made-up.
            b.Compound.RemoveAll(c => fantasySet.Contains(c));

            // Firsts that share the same set of themes share one merged title list. Each list keeps a
            // running count of titles by length, so "how many titles fit after this first name" is two
            // lookups instead of a walk over thousands of titles.
            var merged = new Dictionary<string, (string[] list, int[] upTo)>();
            foreach (var kv in firstThemes)
            {
                string mk = string.Join("|", kv.Value.Select(t => t.Name).OrderBy(n => n));
                if (!merged.TryGetValue(mk, out var m))
                {
                    var list = kv.Value.SelectMany(t => themeTitles[t]).Distinct(StringComparer.Ordinal).ToArray();
                    var upTo = new int[64];                  // upTo[n] = titles with length <= n
                    foreach (var c in list) upTo[Math.Min(c.Length, 63)]++;
                    for (int i = 1; i < upTo.Length; i++) upTo[i] += upTo[i - 1];
                    merged[mk] = m = (list, upTo);
                }
                b.Firsts.Add((kv.Key, m.list, m.upTo));
            }
            baseCache[key] = b;
            return b;
        }

        public NamePool(GameClass cls, Gender gender, int maxLen, string prefix)
        {
            prefix = (prefix ?? "").Trim();
            minLen = 3;
            this.maxLen = maxLen;
            var b = GetBase(cls, gender, prefix);

            foreach (var n in b.Fantasy) if (Fits(n.Length)) { Fantasy.Add(n); fantasySet.Add(n); }
            foreach (var n in b.Compound) if (Fits(n.Length)) { Compound.Add(n); compoundSet.Add(n); }
            foreach (var (first, titles, upTo) in b.Firsts)
            {
                int lo = Math.Max(minLen - first.Length, 1), hi = Math.Min(maxLen - first.Length, 63);
                int fits = hi < lo ? 0 : upTo[hi] - upTo[lo - 1];
                if (fits == 0) continue;
                titlesFor[first] = titles;
                firsts.Add(first);
                TitledTotal += fits;
            }
        }

        bool Fits(int len) => len >= minLen && len <= maxLen;

        public string RandomName(Style style, Random rnd)
        {
            bool f = (style == Style.Mixed || style == Style.Fantasy) && Fantasy.Count > 0;
            bool c = (style == Style.Mixed || style == Style.Compound) && Compound.Count > 0;
            bool t = (style == Style.Mixed || style == Style.NameAndTitle) && firsts.Count > 0;
            double wF = f ? 45 : 0, wC = c ? 35 : 0, wT = t ? 20 : 0;
            if (wF + wC + wT == 0) return null;
            double r = rnd.NextDouble() * (wF + wC + wT);
            if (r < wF) return Fantasy[rnd.Next(Fantasy.Count)];
            if (r < wF + wC) return Compound[rnd.Next(Compound.Count)];
            for (int i = 0; i < 1000; i++)
            {
                string first = firsts[rnd.Next(firsts.Count)];
                var titles = titlesFor[first];
                string name = first + titles[rnd.Next(titles.Length)];
                if (Fits(name.Length)) return name;
            }
            return Fantasy.Count > 0 ? Fantasy[rnd.Next(Fantasy.Count)] : null;
        }

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
            bool fOk = style == Style.Mixed || style == Style.Fantasy;
            bool cOk = style == Style.Mixed || style == Style.Compound;
            bool tOk = style == Style.Mixed || style == Style.NameAndTitle;

            while (result.Count < count)
            {
                if (!fOk && !cOk && !tOk) break;

                // Mixed: roughly 45% made-up, 35% compound, 20% name + title, skipping any that ran out.
                double wF = fOk ? 45 : 0, wC = cOk ? 35 : 0, wT = tOk ? 20 : 0;
                double r = rnd.NextDouble() * (wF + wC + wT);
                string name;
                if (r < wF) { name = NextFrom(Fantasy, seen, rnd); if (name == null) { fOk = false; continue; } }
                else if (r < wF + wC) { name = NextFrom(Compound, seen, rnd); if (name == null) { cOk = false; continue; } }
                else { name = NextTitled(seen, rnd); if (name == null) { tOk = false; continue; } }
                seen.Add(name);
                result.Add(name);
            }
            return result;
        }

        // A random unseen entry. Random picks almost always hit; near the end of a list we scan it.
        static string NextFrom(List<string> list, ISet<string> seen, Random rnd)
        {
            if (list.Count == 0) return null;
            for (int i = 0; i < 200; i++)
            {
                string n = list[rnd.Next(list.Count)];
                if (!seen.Contains(n)) return n;
            }
            var left = list.Where(n => !seen.Contains(n)).ToList();
            return left.Count == 0 ? null : left[rnd.Next(left.Count)];
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
            if (IsVowel(la) && IsVowel(fb))
            {
                if (la == fb) b = b.Substring(1);                          // Thra+ana -> Thrana
                else if (SmoothPairs.Contains("" + la + fb)) { }           // Ka+ion -> Kaion: both vowels stay
                else return a.Substring(0, a.Length - 1) + b;              // Fidmi+ard -> Fidmard (not Fidmird)
            }
            else if (la == fb && !IsVowel(la)) b = b.Substring(1);       // Gor+rok -> Gorok
            return b.Length == 0 ? a : a + b;
        }

        // Vowel pairs that read well across a syllable join.
        static readonly HashSet<string> SmoothPairs = new HashSet<string> { "ae", "ai", "au", "ea", "ei", "ia", "ie", "io", "ua" };

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
