using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArpgNameRandomizer
{
    enum Gender { Any, Male, Female }
    enum Style { Mixed, Fantasy, Compound, NameAndTitle }

    class NameGenerator
    {
        readonly Random rnd = new Random();

        public List<string> Generate(Game game, GameClass cls, Gender gender, Style style,
                                     int maxLen, string prefix, int count,
                                     ISet<string> exclude)
        {
            prefix = (prefix ?? "").Trim();
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int minLen = Math.Max(game.MinLen, 3);

            // Give up after enough misses (tight length + prefix can make some combos impossible).
            for (int attempt = 0; attempt < 40000 && result.Count < count; attempt++)
            {
                Theme t = Pick(cls.Themes);
                Style s = style == Style.Mixed ? PickMixed() : style;

                string name;
                switch (s)
                {
                    case Style.Compound: name = Compound(t); break;
                    case Style.NameAndTitle: name = Titled(t, gender, prefix); break;
                    default: name = Fantasy(t, gender, prefix); break;
                }
                if (name == null) continue;
                name = Cap(name);

                if (name.Length < minLen || name.Length > maxLen) continue;
                if (prefix.Length > 0 && !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                if (exclude != null && exclude.Contains(name)) continue;
                if (!seen.Add(name)) continue;
                result.Add(name);
            }
            return result;
        }

        Style PickMixed()
        {
            double r = rnd.NextDouble();
            return r < 0.45 ? Style.Fantasy : r < 0.80 ? Style.Compound : Style.NameAndTitle;
        }

        // "Mor" + ("ga") + "oth" -> Morgoth-ish made-up names
        string Fantasy(Theme t, Gender g, string prefix)
        {
            string start = Pick(t.Starts);
            // With a prefix, often use it as the opener so we don't rely on luck.
            if (prefix.Length > 0 && rnd.NextDouble() < 0.6) start = prefix;
            string name = start;
            if (rnd.NextDouble() < 0.3) name = Join(name, Pick(t.Mids));
            string[] ends = g == Gender.Male ? t.EndsM
                          : g == Gender.Female ? t.EndsF
                          : (rnd.Next(2) == 0 ? t.EndsM : t.EndsF);
            name = Join(name, Pick(ends));
            return Pronounceable(name) ? name : null;
        }

        // "Bone" + "caller" -> Bonecaller
        string Compound(Theme t)
        {
            string a = Pick(t.WordA), b = Pick(t.WordB);
            if (char.ToLowerInvariant(a[a.Length - 1]) == char.ToLowerInvariant(b[0])) return null; // Steellash
            if (a.Equals(b, StringComparison.OrdinalIgnoreCase)) return null;
            b = b.ToLowerInvariant();
            if (b.Length >= 2 && a.EndsWith(b.Substring(0, 2), StringComparison.OrdinalIgnoreCase)) return null; // Ashshroud
            return a + b;
        }

        // Kael + Duskborn -> KaelDuskborn (letters only, no underscore)
        string Titled(Theme t, Gender g, string prefix)
        {
            string first = Fantasy(t, g, prefix);
            if (first == null || first.Length > 8) return null;
            string title = Compound(t);
            if (title == null) return null;
            return Cap(first) + Cap(title);
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

        T Pick<T>(T[] items) => items[rnd.Next(items.Length)];
    }
}
