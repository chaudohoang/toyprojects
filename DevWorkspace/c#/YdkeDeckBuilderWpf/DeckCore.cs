using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace YdkeDeckBuilderWpf;

// The deck logic, ported from web\YdkeDeckBuilder\index.html. Keep the two in step: the same deck list must
// give the same ydke:// code in both. No WPF types here, so it can be tested from a console app.

public enum Section { Main, Extra, Side }

public sealed record ListEntry(Section Section, int Count, string Name);

public sealed class Card
{
    static readonly HashSet<string> ExtraFrames =
        new() { "fusion", "synchro", "xyz", "link", "fusion_pendulum", "synchro_pendulum", "xyz_pendulum" };

    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("frameType")] public string FrameType { get; set; } = "";
    [JsonPropertyName("ygoprodeck_url")] public string? Url { get; set; }
    [JsonPropertyName("banlist_info")] public Dictionary<string, string>? BanlistInfo { get; set; }

    public bool IsExtraDeck => ExtraFrames.Contains(FrameType);

    /// <summary>"Forbidden", "Limited", "Semi-Limited" or null. <paramref name="format"/> is "tcg", "ocg" or null.</summary>
    public string? BanStatus(string? format) => format == null ? null : BanlistInfo?.GetValueOrDefault("ban_" + format);

    public int Limit(string? format) => BanStatus(format) switch
    {
        "Forbidden" => 0,
        "Limited" => 1,
        "Semi-Limited" => 2,
        _ => 3,
    };
}

public static class DeckListParser
{
    static readonly Regex CountFirst = new(@"^(\d+)\s*[x×]?\s+(.+)$", RegexOptions.IgnoreCase);
    static readonly Regex CountLast = new(@"^(.+?)\s+[x×]\s*(\d+)$", RegexOptions.IgnoreCase);
    static readonly Regex CountParens = new(@"^(.+?)\s+\((\d+)\)$");

    /// <summary>
    /// Lines look like "3 Card Name", "3x Card Name", "Card Name x3", "Card Name (3)" or just "Card Name".
    /// A heading ("Main Deck:", "Extra Deck", "Side Deck:", "Alternative Cards you can play:") switches section.
    /// </summary>
    public static List<ListEntry> Parse(string text)
    {
        var entries = new List<ListEntry>();
        var section = Section.Main;
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith("//")) continue;

            var heading = Regex.Replace(line, @"[:\-=*]+$", "").Trim().ToLowerInvariant();
            if (line.EndsWith(':') || Regex.IsMatch(heading, "^(main|extra|side)( deck)?$") || heading == "!side")
            {
                if (heading.Contains("extra")) { section = Section.Extra; continue; }
                if (Regex.IsMatch(heading, "side|alternative|alternate|optional|tech")) { section = Section.Side; continue; }
                if (heading.Contains("main")) { section = Section.Main; continue; }
            }

            int count = 1;
            var name = line;
            Match m;
            if ((m = CountFirst.Match(line)).Success) { count = int.Parse(m.Groups[1].Value); name = m.Groups[2].Value; }
            else if ((m = CountLast.Match(line)).Success || (m = CountParens.Match(line)).Success)
            {
                name = m.Groups[1].Value;
                count = int.Parse(m.Groups[2].Value);
            }
            entries.Add(new(section, count, name.Trim()));
        }
        return entries;
    }

    /// <summary>Lowercase letters and digits only, with curly quotes straightened first.</summary>
    public static string Norm(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s.ToLowerInvariant().Replace('“', '"').Replace('”', '"').Replace('‘', '\'').Replace('’', '\''))
            if (ch is >= 'a' and <= 'z' or >= '0' and <= '9') sb.Append(ch);
        return sb.ToString();
    }
}

public sealed record LookupResult(Dictionary<string, Card> Found, Dictionary<string, List<string>> Suggestions);

public static class CardApi
{
    const string Api = "https://db.ygoprodeck.com/api/v7/cardinfo.php";
    public static readonly HttpClient Http = CreateClient();

    static HttpClient CreateClient()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("YdkeDeckBuilder/1.0");
        return http;
    }

    sealed class Response
    {
        [JsonPropertyName("data")] public List<Card>? Data { get; set; }
    }

    static async Task<List<Card>> GetAsync(string query, CancellationToken ct)
    {
        using var res = await Http.GetAsync($"{Api}?{query}", ct);
        if (res.StatusCode == HttpStatusCode.BadRequest) return new(); // "No card matching your query was found"
        if (!res.IsSuccessStatusCode) throw new HttpRequestException($"YGOPRODeck returned {(int)res.StatusCode}");
        return (await res.Content.ReadFromJsonAsync<Response>(ct))?.Data ?? new();
    }

    /// <summary>
    /// Exact names in batched requests (names that don't exist are silently left out), then a fuzzy search
    /// for each name that missed. fname= is a plain substring match that fails on punctuation (fname=maxx c
    /// finds nothing for Maxx "C"), so it searches on the longest word and picks the closest name.
    /// </summary>
    public static async Task<LookupResult> LookupAsync(IEnumerable<string> names, CancellationToken ct = default)
    {
        var unique = names.Distinct().ToList();
        // Normalized names collide for different cards ("Hero Flash!!" / "H.E.R.O. Flash!", "Raimei" / "Rai-Mei"),
        // so the exact spelling is tried first.
        var byNorm = new Dictionary<string, Card>();
        var byExact = new Dictionary<string, Card>(StringComparer.OrdinalIgnoreCase);
        foreach (var batch in unique.Chunk(40))
            foreach (var card in await GetAsync("name=" + Uri.EscapeDataString(string.Join("|", batch)), ct))
            {
                byNorm[DeckListParser.Norm(card.Name)] = card;
                byExact[card.Name] = card;
            }

        var found = new Dictionary<string, Card>();
        var suggestions = new Dictionary<string, List<string>>();
        foreach (var name in unique)
        {
            if (byExact.TryGetValue(name, out var exact) || byNorm.TryGetValue(DeckListParser.Norm(name), out exact))
            {
                found[name] = exact;
                continue;
            }

            var words = Regex.Replace(name, @"[^\p{L}\p{N}' -]", " ")
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 3)
                .OrderByDescending(w => w.Length)
                .Take(2);
            var candidates = new List<Card>();
            foreach (var w in words)
            {
                candidates = await GetAsync("fname=" + Uri.EscapeDataString(w), ct);
                if (candidates.Count > 0) break;
            }

            var want = DeckListParser.Norm(name);
            var scored = candidates.Select(c => (Card: c, Score: Similarity(want, DeckListParser.Norm(c.Name))))
                .OrderByDescending(s => s.Score).ToList();
            if (scored.Count > 0 && scored[0].Score >= 0.85) found[name] = scored[0].Card;
            else suggestions[name] = scored.Take(3).Select(s => s.Card.Name).ToList();
        }
        return new(found, suggestions);
    }

    /// <summary>1 - normalized Levenshtein distance.</summary>
    public static double Similarity(string a, string b)
    {
        if (a == b) return 1;
        var prev = Enumerable.Range(0, b.Length + 1).ToArray();
        for (int i = 1; i <= a.Length; i++)
        {
            int diag = prev[0];
            prev[0] = i;
            for (int j = 1; j <= b.Length; j++)
            {
                int tmp = prev[j];
                prev[j] = Math.Min(Math.Min(prev[j] + 1, prev[j - 1] + 1), diag + (a[i - 1] == b[j - 1] ? 0 : 1));
                diag = tmp;
            }
        }
        return 1 - (double)prev[b.Length] / Math.Max(a.Length, b.Length);
    }
}

public sealed class DeckLine
{
    public required Card Card { get; init; }
    public int Count { get; set; }
}

public enum IssueKind { Error, Warning, Info }

public sealed record Issue(IssueKind Kind, string Text);

public sealed class Deck
{
    public List<DeckLine> Main { get; } = new();
    public List<DeckLine> Extra { get; } = new();
    public List<DeckLine> Side { get; } = new();

    public List<DeckLine> this[Section s] => s switch { Section.Main => Main, Section.Extra => Extra, _ => Side };

    public int Size(Section s) => this[s].Sum(l => l.Count);

    IEnumerable<int> Ids(Section s) => this[s].SelectMany(l => Enumerable.Repeat(l.Card.Id, l.Count));

    /// <summary>ydke://main!extra!side! - each section is its passcodes as little-endian uint32s, base64-encoded.</summary>
    public string ToYdke()
    {
        static string Encode(IEnumerable<int> ids)
        {
            var list = ids.ToList();
            var bytes = new byte[list.Count * 4];
            for (int i = 0; i < list.Count; i++) BitConverter.TryWriteBytes(bytes.AsSpan(i * 4), (uint)list[i]);
            return Convert.ToBase64String(bytes);
        }
        if (!BitConverter.IsLittleEndian) throw new PlatformNotSupportedException("ydke encoding assumes a little-endian CPU.");
        return $"ydke://{Encode(Ids(Section.Main))}!{Encode(Ids(Section.Extra))}!{Encode(Ids(Section.Side))}!";
    }

    /// <summary>The .ydk text EDOPro and YGOPro read.</summary>
    public string ToYdk()
    {
        var sb = new StringBuilder("#created by YDKE Deck Builder\n#main\n");
        foreach (var id in Ids(Section.Main)) sb.Append(id).Append('\n');
        sb.Append("#extra\n");
        foreach (var id in Ids(Section.Extra)) sb.Append(id).Append('\n');
        sb.Append("!side\n");
        foreach (var id in Ids(Section.Side)) sb.Append(id).Append('\n');
        return sb.ToString();
    }
}

public static class DeckBuilder
{
    public static (Deck Deck, List<Issue> Issues) Build(
        List<ListEntry> entries, LookupResult lookup, string? format, bool moveExtraDeckMonsters)
    {
        var issues = new List<Issue>();
        var deck = new Deck();

        foreach (var e in entries)
        {
            if (!lookup.Found.TryGetValue(e.Name, out var card))
            {
                var s = lookup.Suggestions.GetValueOrDefault(e.Name) ?? new();
                issues.Add(new(IssueKind.Error, $"Not found: “{e.Name}”" +
                    (s.Count > 0 ? $". Did you mean {string.Join(", ", s.Select(n => $"“{n}”"))}?" : ".")));
                continue;
            }
            if (DeckListParser.Norm(card.Name) != DeckListParser.Norm(e.Name))
                issues.Add(new(IssueKind.Info, $"“{e.Name}” read as “{card.Name}”."));

            var section = e.Section;
            if (moveExtraDeckMonsters && section != Section.Side)
            {
                if (section == Section.Main && card.IsExtraDeck)
                {
                    section = Section.Extra;
                    issues.Add(new(IssueKind.Info, $"{card.Name} is an Extra Deck monster: moved to the Extra Deck."));
                }
                else if (section == Section.Extra && !card.IsExtraDeck)
                {
                    section = Section.Main;
                    issues.Add(new(IssueKind.Info, $"{card.Name} isn't an Extra Deck monster: moved to the Main Deck."));
                }
            }
            var existing = deck[section].FirstOrDefault(l => l.Card.Id == card.Id);
            if (existing != null) existing.Count += e.Count;
            else deck[section].Add(new DeckLine { Card = card, Count = e.Count });
        }

        // Deck-size rules, then copy limits; both go to the top of the list.
        int m = deck.Size(Section.Main), x = deck.Size(Section.Extra), sd = deck.Size(Section.Side);
        if (m < 40) issues.Insert(0, new(IssueKind.Error, $"Main Deck has {m} cards; it needs at least 40."));
        if (m > 60) issues.Insert(0, new(IssueKind.Error, $"Main Deck has {m} cards; the maximum is 60."));
        if (x > 15) issues.Insert(0, new(IssueKind.Error, $"Extra Deck has {x} cards; the maximum is 15."));
        if (sd > 15) issues.Insert(0, new(IssueKind.Error, $"Side Deck has {sd} cards; the maximum is 15."));

        var totals = new List<(Card Card, int Count)>();
        foreach (var line in deck.Main.Concat(deck.Extra).Concat(deck.Side))
        {
            int i = totals.FindIndex(t => t.Card.Id == line.Card.Id);
            if (i < 0) totals.Add((line.Card, line.Count));
            else totals[i] = (totals[i].Card, totals[i].Count + line.Count);
        }
        foreach (var (card, count) in totals)
        {
            int limit = card.Limit(format);
            if (count <= limit) continue;
            var why = limit == 3
                ? "the limit is 3 copies"
                : $"it is {card.BanStatus(format)} in {format!.ToUpperInvariant()} (max {limit})";
            issues.Insert(0, new(limit == 3 ? IssueKind.Error : IssueKind.Warning,
                $"{count}× {card.Name} across Main/Extra/Side, but {why}."));
        }
        return (deck, issues);
    }
}
