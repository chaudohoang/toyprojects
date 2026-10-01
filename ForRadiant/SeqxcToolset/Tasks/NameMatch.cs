using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SeqxcToolset.Tasks
{
    /// <summary>
    /// Ranks how well an imported pattern name matches a real PatternSetupName, so every
    /// task's match picker orders its candidates the same way.
    ///
    /// Lived as a private copy in the two pattern tasks until Exposure Time and
    /// Luminance Scale needed a picker too; four copies of a scoring rule would drift.
    /// </summary>
    public static class NameMatch
    {
        /// <summary>
        /// Rough relevance score between an imported name (e.g. "R31") and a real
        /// PatternSetupName (e.g. "W31_step23_R"): tokenizes both into letter- and
        /// digit-runs and rewards shared tokens, with extra weight for a shared trailing
        /// letter token (the common "_R"/"_G"/"_B" channel suffix convention) and shared
        /// numbers. An exact match scores 1000 so it always sorts first.
        /// </summary>
        public static int Score(string query, string candidateName)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(candidateName)) return 0;

            string q = query.Trim();
            string c = candidateName.Trim();
            if (c.Equals(q, StringComparison.OrdinalIgnoreCase)) return 1000;

            var qTokens = Regex.Matches(q, @"[A-Za-z]+|\d+").Cast<Match>().Select(m => m.Value).ToList();
            var cTokens = Regex.Matches(c, @"[A-Za-z]+|\d+").Cast<Match>().Select(m => m.Value).ToList();

            int score = 0;
            foreach (var qt in qTokens)
                foreach (var ct in cTokens)
                    if (string.Equals(qt, ct, StringComparison.OrdinalIgnoreCase))
                        score += char.IsDigit(qt[0]) ? 30 : 15;

            var qLastLetterToken = qTokens.LastOrDefault(t => char.IsLetter(t[0]));
            var cLastToken = cTokens.LastOrDefault();
            if (qLastLetterToken != null && cLastToken != null &&
                string.Equals(qLastLetterToken, cLastToken, StringComparison.OrdinalIgnoreCase))
                score += 25;

            if (c.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) score += 10;
            if (q.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0) score += 5;

            return score;
        }
    }
}
