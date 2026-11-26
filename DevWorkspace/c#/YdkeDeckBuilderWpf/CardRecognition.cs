using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YdkeDeckBuilderWpf;

// Reads a deck from a screenshot (Master Duel's deck screen and similar card grids). Names on screenshots
// are usually far too small to read, so it finds each card in the grid and matches the middle of its
// artwork against YGOPRODeck's card images, which ArtIndex keeps as tiny colour thumbnails.

/// <summary>A decoded image as 8-bit BGRA rows.</summary>
public sealed class Pixels
{
    public int Width { get; }
    public int Height { get; }
    public byte[] Bgra { get; }

    Pixels(int width, int height, byte[] bgra) { Width = width; Height = height; Bgra = bgra; }

    public static Pixels From(BitmapSource source)
    {
        var bmp = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var px = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
        bmp.CopyPixels(px, bmp.PixelWidth * 4, 0);
        return new(bmp.PixelWidth, bmp.PixelHeight, px);
    }

    /// <summary>
    /// PNG, JPEG, BMP, GIF, and WebP where Windows has the WebP codec (Windows 11 does). BitmapImage rather
    /// than BitmapDecoder: the decoder throws "Unexpected property type or value" on JPEGs whose EXIF block it
    /// doesn't like, which is three in four of MDPro's Picture\Art files.
    /// </summary>
    public static BitmapSource Decode(Stream stream)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.StreamSource = stream;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        image.EndInit();
        image.Freeze();
        return image;
    }

    public static Pixels Load(Stream stream) => From(Decode(stream));

    /// <summary>Brightest channel, 0-255.</summary>
    public int Value(int x, int y)
    {
        int i = (y * Width + x) * 4;
        return Math.Max(Bgra[i], Math.Max(Bgra[i + 1], Bgra[i + 2]));
    }
}

/// <summary>
/// A card on a screenshot. <see cref="Visible"/> is how much of it shows from the left (1 = all of it): when a
/// row has more cards than fit, Master Duel overlaps them, each covering the right part of the one before.
/// </summary>
public sealed record FoundCard(Int32Rect Rect, int Section, double Visible);

public static class CardFinder
{
    const int Dark = 95;           // the deck screen's background stays below this; every card frame doesn't
    const double CardAspect = 0.686; // 59 × 86 mm

    /// <summary>
    /// Cards in reading order, with the index of the section each one is in (sections are separated by a
    /// header, i.e. a gap noticeably taller than the gaps between rows).
    /// Rows are runs of image rows with bright pixels; in each row, cards are runs of columns with bright pixels.
    /// A run too wide for one card is split evenly, since the gap between cards can be a pixel or two. A row
    /// where that doesn't work out (runs that aren't a whole number of cards: the cards overlap, and dark
    /// frames such as Xyz break up the bright columns) is laid out from the spacing its card edges repeat at.
    /// </summary>
    public static List<FoundCard> Find(Pixels img)
    {
        var bright = new int[img.Height];
        for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
                if (img.Value(x, y) > Dark) bright[y]++;

        // Card rows are the tallest bands; headers, buttons and text are much shorter.
        var bands = Runs(img.Height, y => bright[y] > img.Width / 20).Where(b => b.Length >= 24).ToList();
        if (bands.Count == 0) return new();
        int tallest = bands.Max(b => b.Length);
        var tall = bands.Select(b => b.Length).Where(l => l >= tallest * 0.6).OrderBy(l => l).ToList();
        int rowHeight = tall[tall.Count / 2];

        // The cards in one row band: side by side, or overlapped (see Overlapped). Only ones with a text box count.
        List<(Int32Rect Rect, int Row, double Visible)> Row(int top, int height)
        {
            var cols = new int[img.Width];
            for (int x = 0; x < img.Width; x++)
                for (int y = top; y < top + height; y++)
                    if (img.Value(x, y) > Dark) cols[x]++;

            double cardWidth = height * CardAspect;
            // Runs narrower than most of a card are scroll bars, icons and the like.
            var runs = Runs(img.Width, x => cols[x] > height / 8).Where(r => r.Length > cardWidth * 0.6).ToList();
            if (runs.Count == 0) return new();
            var pitched = Pitched(img, top, height, runs[0].Start, runs[^1].Start + runs[^1].Length, rowHeight);
            var byPitch = pitched?.Cards.Select(c => (c.Rect, top, c.Visible)).ToList() ?? new();
            if (pitched?.Pitch < cardWidth * 0.9) return byPitch; // overlapped: the runs mean nothing

            var row = new List<(Int32Rect, int, double)>();
            foreach (var (left, width) in runs)
            {
                int n = (int)Math.Round(width / cardWidth);
                if (n < 1 || Math.Abs(width / (double)n / height - CardAspect) > 0.12) continue;
                for (int i = 0; i < n; i++)
                {
                    int x0 = left + width * i / n, x1 = left + width * (i + 1) / n;
                    var rect = Trim(img, new Int32Rect(x0, top, x1 - x0, height), rowHeight);
                    if (HasTextBox(img, rect, 1)) row.Add((rect, top, 1));
                }
            }
            // A dark frame (Xyz) can break up or shrink a card's bright run; the pitch layout doesn't mind.
            return byPitch.Count > row.Count ? byPitch : row;
        }

        var cards = new List<(Int32Rect Rect, int Row, double Visible)>();
        foreach (var (top, height) in bands)
        {
            if (Math.Abs(height - rowHeight) <= rowHeight * 0.15) cards.AddRange(Row(top, height));
            // Taller: something touching the row (a video's caption banner, say) made one band of both. The cards
            // are at its top or its bottom; whichever finds more is the row.
            else if (height > rowHeight && height < rowHeight * 2)
                cards.AddRange(new[] { Row(top, rowHeight), Row(top + height - rowHeight, rowHeight) }.MaxBy(r => r.Count)!);
        }
        if (cards.Count == 0) return new();

        // Keep the common card size: drops icons, deck-name boxes and the like that happen to be card-shaped.
        var h = cards.Select(c => c.Rect.Height).OrderBy(v => v).ElementAt(cards.Count / 2);
        cards.RemoveAll(c => Math.Abs(c.Rect.Height - h) > h * 0.15);

        var result = new List<FoundCard>();
        int section = 0, prevTop = -1, rowGap = int.MaxValue;
        foreach (var row in cards.GroupBy(c => c.Row).OrderBy(g => g.Key))
        {
            if (prevTop >= 0)
            {
                int step = row.Key - prevTop;
                if (step > rowGap * 1.35) section++;
                rowGap = Math.Min(rowGap, step);
            }
            prevTop = row.Key;
            foreach (var c in row.OrderBy(c => c.Rect.X)) result.Add(new FoundCard(c.Rect, section, c.Visible));
        }
        return result;
    }

    /// <summary>
    /// The cards of a row whose bright part spans [left, right), laid out at the pitch where the row's vertical
    /// edges (each card's left border and art box) repeat, found by autocorrelation. Below a card's width the
    /// cards overlap, each covering the right part of the one before, and the last is whole; above it they sit
    /// side by side with a gap. Slots carry on past [left, right) while they hold a card, since a card with a
    /// dark frame can fall outside the bright part; only slots with a card's text box are kept. Null if the
    /// edges don't repeat.
    /// </summary>
    internal static (double Pitch, List<(Int32Rect Rect, double Visible)> Cards)? Pitched(Pixels img, int top, int height, int left, int right, int cardHeight)
    {
        double cardWidth = height * CardAspect;
        int span = right - left;
        if (span < cardWidth * 1.5) return null;

        var edges = new double[span];
        for (int x = left + 1; x < right - 1; x++)
            for (int y = top; y < top + height; y++)
                edges[x - left] += Math.Abs(img.Value(x + 1, y) - img.Value(x - 1, y));
        double mean = edges.Average();
        for (int i = 0; i < span; i++) edges[i] -= mean;

        double Acf(int lag)
        {
            double s = 0;
            for (int i = 0; i + lag < span; i++) s += edges[i] * edges[i + lag];
            return s / (span - lag);
        }
        // The highest peak, not the highest value: near the low end the curve is still coming down from lag 0.
        int Peak(int from, int to)
        {
            int best = 0;
            double bestScore = double.MinValue;
            for (int lag = Math.Max(from, 1); lag <= to && lag + 1 < span; lag++)
            {
                double s = Acf(lag);
                if (s > bestScore && s > Acf(lag - 1) && s >= Acf(lag + 1)) { bestScore = s; best = lag; }
            }
            return best;
        }

        // Master Duel never covers more than half a card; closer repeats are text and the like.
        int rough = Peak((int)(cardWidth * 0.5), (int)(cardWidth * 1.4));
        if (rough == 0) return null;

        // One peak is only good to a pixel, which over 14 gaps can miscount by one. The peak k pitches out,
        // divided by k, is k times more precise.
        double pitch = rough;
        int k = Math.Min(4, (int)((span - cardWidth) / rough));
        if (k >= 2 && Peak(k * rough - k, k * rough + k) is var far and > 0) pitch = far / (double)k;

        // Not re-derived from the span: a dark frame (Xyz) ends the bright part short of the last card's edge.
        int n = (int)Math.Round((span - cardWidth) / pitch) + 1;

        double visible = Math.Min(1, pitch / cardWidth);
        var slots = new List<(Int32Rect Rect, double Visible)>();
        Int32Rect? Slot(int i)
        {
            int x = (int)Math.Round(left + i * pitch), w = (int)Math.Round(cardWidth);
            return x < 0 || x + w > img.Width ? null : Trim(img, new Int32Rect(x, top, w, height), cardHeight);
        }
        for (int i = -1; Slot(i) is { } r && HasTextBox(img, r, visible); i--) slots.Insert(0, (r, visible));
        for (int i = 0; i < n || Slot(i) is { } r && HasTextBox(img, r, visible); i++)
            if (Slot(i) is { } s && HasTextBox(img, s, i == n - 1 ? 1 : visible)) slots.Add((s, visible));
        if (slots.Count > 0) slots[^1] = (slots[^1].Rect, 1); // nothing lies over the last card
        return (pitch, slots);
    }

    static IEnumerable<(int Start, int Length)> Runs(int length, Func<int, bool> on)
    {
        int start = -1;
        for (int i = 0; i <= length; i++)
        {
            bool b = i < length && on(i);
            if (b && start < 0) start = i;
            else if (!b && start >= 0) { yield return (start, i - start); start = -1; }
        }
    }

    /// <summary>
    /// Every card, whatever its frame (Xyz and Link included) and even Master Duel's full-art ones, has a calm,
    /// pale text box across the bottom: small dark print on light grey. On eight screenshots 69-100 % of it was
    /// light, greyish pixels, with a brightness spread (standard deviation) of at most 66. "Cards" cut out of a
    /// video's caption banner had 0-63 % light pixels (whitish lettering got that high), but a spread of 96-116:
    /// big letters on black. The box spans x 10-90 %, y 72-92 % of the card; for an overlapped card only the part
    /// that can be seen is checked.
    /// </summary>
    static bool HasTextBox(Pixels img, Int32Rect r, double visible)
    {
        double w = r.Height * CardAspect;
        int x0 = (int)(r.X + 0.10 * w), x1 = Math.Min(img.Width, (int)(r.X + Math.Min(0.90, visible - 0.05) * w));
        int y0 = (int)(r.Y + 0.72 * r.Height), y1 = Math.Min(img.Height, (int)(r.Y + 0.92 * r.Height));
        int n = 0, light = 0;
        double sum = 0, sumSq = 0;
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
            {
                int i = (y * img.Width + x) * 4;
                int max = Math.Max(img.Bgra[i], Math.Max(img.Bgra[i + 1], img.Bgra[i + 2]));
                int min = Math.Min(img.Bgra[i], Math.Min(img.Bgra[i + 1], img.Bgra[i + 2]));
                n++;
                sum += max;
                sumSq += max * max;
                if (max > 140 && max - min < 90) light++;
            }
        if (n == 0) return false;
        double mean = sum / n, spread = Math.Sqrt(Math.Max(0, sumSq / n - mean * mean));
        return light >= n / 2 && spread < 85;
    }

    /// <summary>
    /// A row band is as tall as its tallest card, or taller when something touches the row (a caption banner);
    /// shrink each card to its own bright rows. But not below <paramref name="cardHeight"/>, the usual card
    /// height: a dark frame (Xyz) has dark top and bottom edges that would trim it to ~80 % and get it dropped
    /// as the wrong size. Then the band is the better guess.
    /// </summary>
    static Int32Rect Trim(Pixels img, Int32Rect r, int cardHeight)
    {
        bool RowOn(int y)
        {
            int n = 0;
            for (int x = r.X; x < r.X + r.Width; x++) if (img.Value(x, y) > Dark) n++;
            return n > r.Width / 4;
        }
        int top = r.Y, bottom = r.Y + r.Height;
        while (top < bottom - 1 && !RowOn(top)) top++;
        while (bottom > top + 1 && !RowOn(bottom - 1)) bottom--;
        return bottom - top < cardHeight * 0.92 ? r : new Int32Rect(r.X, top, r.Width, bottom - top);
    }
}

/// <summary>
/// A grid of average colours over the middle of a card's artwork. The region is well inside the art box of
/// every frame (pendulum included) and clear of Master Duel's rarity and limit badges in the top corners.
/// </summary>
public static class ArtPrint
{
    public const int Grid = 12;
    public const int Length = Grid * Grid * 3;
    const double X0 = 0.21, X1 = 0.79, Y0 = 0.25, Y1 = 0.61;

    public static byte[] Of(Pixels img, double x, double y, double w, double h)
    {
        double ax = x + X0 * w, ay = y + Y0 * h, aw = (X1 - X0) * w, ah = (Y1 - Y0) * h;
        var sums = new double[Length];
        var counts = new int[Grid * Grid];
        int px0 = Math.Max(0, (int)ax), px1 = Math.Min(img.Width, (int)Math.Ceiling(ax + aw));
        int py0 = Math.Max(0, (int)ay), py1 = Math.Min(img.Height, (int)Math.Ceiling(ay + ah));
        for (int py = py0; py < py1; py++)
        {
            int gy = (int)((py + 0.5 - ay) / ah * Grid);
            if (gy is < 0 or >= Grid) continue;
            for (int px = px0; px < px1; px++)
            {
                int gx = (int)((px + 0.5 - ax) / aw * Grid);
                if (gx is < 0 or >= Grid) continue;
                int cell = gy * Grid + gx, i = (py * img.Width + px) * 4;
                sums[cell * 3] += img.Bgra[i + 2];
                sums[cell * 3 + 1] += img.Bgra[i + 1];
                sums[cell * 3 + 2] += img.Bgra[i];
                counts[cell]++;
            }
        }
        var print = new byte[Length];
        for (int i = 0; i < Length; i++)
            print[i] = (byte)Math.Round(counts[i / 3] == 0 ? 0 : sums[i] / counts[i / 3]);
        return print;
    }

    /// <summary>
    /// For an image of the artwork alone: placed where the art box sits on a card, x 12-88 % and from y 18.2 %
    /// (a pendulum's is wider, x 6.7-93.3 %, and its art runs on behind the text boxes, so it's taller than
    /// wide: 712 × 908), then printed as a card.
    /// </summary>
    public static byte[] OfArtOnly(Pixels art)
    {
        bool pendulum = art.Height > art.Width * 1.15;
        double w = art.Width / (pendulum ? 0.866 : 0.76), h = w * 86 / 59;
        return Of(art, -(pendulum ? 0.067 : 0.12) * w, -0.182 * h, w, h);
    }

    /// <summary>
    /// The print values (indexes into a print) that lie on the visible part of a card whose left
    /// <paramref name="visible"/> fraction can be seen; null when it's all visible. Stops a little short of the
    /// edge, where the next card's border and shadow fall.
    /// </summary>
    public static int[]? VisibleDims(double visible)
    {
        if (visible >= 0.97) return null;
        int cols = 0;
        while (cols < Grid && X0 + (cols + 1) * (X1 - X0) / Grid <= visible - 0.03) cols++;
        cols = Math.Max(cols, 4);
        return Enumerable.Range(0, Length).Where(i => i / 3 % Grid < cols).ToArray();
    }

    /// <summary>Normalize over <paramref name="dims"/> only; the rest are zero.</summary>
    public static float[] Normalize(byte[] print, int[] dims)
    {
        var v = new float[Length];
        float mean = 0;
        foreach (var d in dims) mean += print[d];
        mean /= dims.Length;
        double norm = 0;
        foreach (var d in dims) { v[d] = print[d] - mean; norm += v[d] * v[d]; }
        var scale = norm > 0 ? (float)(1 / Math.Sqrt(norm)) : 0;
        foreach (var d in dims) v[d] *= scale;
        return v;
    }

    /// <summary>
    /// The dot product of <paramref name="q"/> (from Normalize(print, dims)) with <paramref name="print"/>
    /// normalized over the same dims, without building that vector: q sums to zero over them, so the mean drops out.
    /// </summary>
    public static float Score(float[] q, byte[] print, int[] dims)
    {
        double sum = 0, sumSq = 0, dot = 0;
        foreach (var d in dims)
        {
            double r = print[d];
            sum += r;
            sumSq += r * r;
            dot += q[d] * r;
        }
        var variance = sumSq - sum * sum / dims.Length;
        return variance > 0 ? (float)(dot / Math.Sqrt(variance)) : 0;
    }

    /// <summary>
    /// The score over <paramref name="dims"/> (all when null) without the two grid rows that match worst.
    /// Things drawn over a screenshot, such as a video's progress line or the edge of a caption, are mostly
    /// horizontal and spoil a row or two of the print. Dropping them lifts the right card from ~0.55 to ~0.9 on
    /// such a screenshot and leaves a clean card's score about the same, while a wrong card still disagrees
    /// everywhere and stays around 0.5-0.65 (highest seen 0.77, next to a 0.96 right one).
    /// </summary>
    /// <summary>
    /// Each colour channel zero-mean and unit-length on its own (over <paramref name="dims"/>, all when null),
    /// scaled so a dot product of two is the average of the three channels' correlations. A colour tint or
    /// shimmer over the whole art, such as Master Duel's foil effect, cancels out; the shapes in each channel
    /// remain.
    /// </summary>
    public static float[] NormalizePerChannel(byte[] print, int[]? dims)
    {
        var v = new float[Length];
        for (int ch = 0; ch < 3; ch++)
        {
            var cd = (dims ?? AllDims).Where(d => d % 3 == ch).ToArray();
            var n = Normalize(print, cd);
            foreach (var d in cd) v[d] = n[d] / MathF.Sqrt(3);
        }
        return v;
    }

    public static float TrimmedScore(byte[] query, byte[] reference, int[]? dims)
    {
        dims ??= AllDims;
        var q = Normalize(query, dims);
        var r = Normalize(reference, dims);
        var err = new double[Grid];
        foreach (var d in dims)
        {
            double e = q[d] - r[d];
            err[d / 3 / Grid] += e * e;
        }
        var worst = Enumerable.Range(0, Grid).OrderByDescending(y => err[y]).Take(2).ToHashSet();
        var kept = dims.Where(d => !worst.Contains(d / 3 / Grid)).ToArray();
        return Score(Normalize(query, kept), reference, kept);
    }

    static readonly int[] AllDims = Enumerable.Range(0, Length).ToArray();

    /// <summary>Zero-mean, unit-length, so a match is a dot product and brightness or contrast shifts don't count.</summary>
    public static float[] Normalize(byte[] print)
    {
        var v = new float[Length];
        float mean = 0;
        foreach (var b in print) mean += b;
        mean /= Length;
        double norm = 0;
        for (int i = 0; i < Length; i++) { v[i] = print[i] - mean; norm += v[i] * v[i]; }
        var scale = norm > 0 ? (float)(1 / Math.Sqrt(norm)) : 0;
        for (int i = 0; i < Length; i++) v[i] *= scale;
        return v;
    }
}

public sealed record ArtEntry(int ImageId, int CardId, string Name, byte[] Print);

public sealed record ArtMatch(ArtEntry Entry, float Score);

/// <summary>
/// One ArtPrint per YGOPRODeck card image (alternate artworks included). The app ships with one in
/// art-index.bin. When YGOPRODeck has images it doesn't, they're downloaded into art\ and the updated
/// index is saved as art-index.bin, both next to the exe, or in %LOCALAPPDATA%\YdkeDeckBuilder if that
/// folder isn't writable (e.g. Program Files). An image already in art\ is never downloaded again.
/// </summary>
public sealed class ArtIndex
{
    const string Magic = "YDKART3"; // 2: tokens left out, 3: Skill Cards too. An older file fails to read and is rebuilt.
    const string ImageUrl = "https://images.ygoprodeck.com/images/cards_small/{0}.jpg";

    const string FileName = "art-index.bin";

    /// <summary>Where saved data goes: next to the exe, else %LOCALAPPDATA%\YdkeDeckBuilder.</summary>
    internal static readonly string[] DataDirs =
    {
        AppContext.BaseDirectory,
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YdkeDeckBuilder"),
    };

    public List<ArtEntry> Entries { get; } = new();
    public DateTime Updated { get; set; }
    float[][] _normalized = Array.Empty<float[]>();
    float[][]? _perChannel; // for MatchIgnoringColour, built on first use

    public static ArtIndex Read(Stream stream)
    {
        using var z = new BrotliStream(stream, CompressionMode.Decompress);
        using var r = new BinaryReader(z, Encoding.UTF8);
        if (r.ReadString() != Magic || r.ReadInt32() != ArtPrint.Grid) throw new InvalidDataException("Not an art index.");
        var index = new ArtIndex { Updated = DateTime.FromBinary(r.ReadInt64()) };
        int n = r.ReadInt32();
        for (int i = 0; i < n; i++)
            index.Entries.Add(new(r.ReadInt32(), r.ReadInt32(), r.ReadString(), r.ReadBytes(ArtPrint.Length)));
        index.Prepare();
        return index;
    }

    public void Write(Stream stream)
    {
        using var z = new BrotliStream(stream, CompressionLevel.SmallestSize);
        using var w = new BinaryWriter(z, Encoding.UTF8);
        w.Write(Magic);
        w.Write(ArtPrint.Grid);
        w.Write(Updated.ToBinary());
        w.Write(Entries.Count);
        foreach (var e in Entries) { w.Write(e.ImageId); w.Write(e.CardId); w.Write(e.Name); w.Write(e.Print); }
    }

    /// <summary>Call after changing Entries.</summary>
    public void Prepare()
    {
        _normalized = Entries.Select(e => ArtPrint.Normalize(e.Print)).ToArray();
        _perChannel = null;
    }

    /// <summary>
    /// Like Match, but each colour channel compared on its own (ArtPrint.NormalizePerChannel), for a card
    /// drawn with Master Duel's foil shimmer. Weaker at telling cards apart, so only for cards Match can't
    /// place (see ScreenshotReader.FoilSure).
    /// </summary>
    public List<ArtMatch> MatchIgnoringColour(Pixels img, Int32Rect card, double visible = 1, int count = 3)
    {
        var keep = ArtPrint.VisibleDims(visible);
        float[] Ref(int i) => keep == null
            ? (_perChannel ??= Entries.AsParallel().AsOrdered().Select(e => ArtPrint.NormalizePerChannel(e.Print, null)).ToArray())[i]
            : ArtPrint.NormalizePerChannel(Entries[i].Print, keep);
        float[] Query(double x, double y) => ArtPrint.NormalizePerChannel(ArtPrint.Of(img, x, y, card.Width, card.Height), keep);

        var q = Query(card.X, card.Y);
        var scores = new float[Entries.Count];
        Parallel.For(0, Entries.Count, i => scores[i] = Dot(q, Ref(i)));
        var shortlist = Enumerable.Range(0, scores.Length).OrderByDescending(i => scores[i]).Take(60).ToList();

        double step = Math.Max(1, card.Width / 40.0);
        var shifted = new List<float[]>();
        for (int dy = -2; dy <= 2; dy++)
            for (int dx = -2; dx <= 2; dx++)
                shifted.Add(Query(card.X + dx * step, card.Y + dy * step));
        return shortlist
            .Select(i => new ArtMatch(Entries[i], shifted.Max(s => Dot(s, Ref(i)))))
            .OrderByDescending(m => m.Score)
            .DistinctBy(m => m.Entry.CardId)
            .Take(count)
            .ToList();
    }

    /// <summary>The newest of the saved indexes and the one built into the exe.</summary>
    public static ArtIndex Load()
    {
        var found = ReadSaved(FileName);
        var res = Application.GetResourceStream(new Uri("pack://application:,,,/" + FileName));
        using (res.Stream) found.Add(Read(res.Stream));
        return found.MaxBy(i => i.Updated)!;
    }

    /// <summary>Every readable copy of <paramref name="fileName"/> in the data folders.</summary>
    internal static List<ArtIndex> ReadSaved(string fileName)
    {
        var found = new List<ArtIndex>();
        foreach (var path in DataDirs.Select(d => Path.Combine(d, fileName)))
        {
            try
            {
                if (!File.Exists(path)) continue;
                using var f = File.OpenRead(path);
                found.Add(Read(f));
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or EndOfStreamException or UnauthorizedAccessException) { }
        }
        return found;
    }

    sealed class CardInfo
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("type")] public string Type { get; set; } = "";
        [JsonPropertyName("card_images")] public List<CardImage>? Images { get; set; }
    }

    sealed class CardImage
    {
        [JsonPropertyName("id")] public int Id { get; set; }
    }

    sealed class CardList
    {
        [JsonPropertyName("data")] public List<CardInfo>? Data { get; set; }
    }

    /// <summary>
    /// Adds prints for card images YGOPRODeck has and the index doesn't, and picks up renamed cards.
    /// Each image comes from art\ if it's there, else it's downloaded (about 12 a second, well inside
    /// YGOPRODeck's limit) and kept in art\.
    /// </summary>
    public async Task<int> UpdateAsync(HttpClient http, IProgress<(int Done, int Total)>? progress, CancellationToken ct)
    {
        var list = await http.GetFromJsonAsync<CardList>("https://db.ygoprodeck.com/api/v7/cardinfo.php", ct);
        var cards = list?.Data ?? new();
        var have = Entries.ToDictionary(e => e.ImageId);
        var todo = new List<(int ImageId, CardInfo Card)>();
        // Tokens and Speed Duel Skill Cards never go in a deck, and share artwork with real cards (Diagram Token
        // is Exceptional Schedule's art, "Power Bond (Skill Card)" is Power Bond's), so they'd only tie with them.
        foreach (var card in cards.Where(c => c.Type is not ("Token" or "Skill Card")))
            foreach (var image in card.Images ?? new())
            {
                if (!have.TryGetValue(image.Id, out var e)) todo.Add((image.Id, card));
                else if (e.Name != card.Name || e.CardId != card.Id)
                    Entries[Entries.IndexOf(e)] = e with { Name = card.Name, CardId = card.Id };
            }

        int done = 0;
        using var gate = new SemaphoreSlim(6);
        var added = new List<ArtEntry>();
        var nextStart = DateTime.MinValue;
        var tasks = todo.Select(async t =>
        {
            await gate.WaitAsync(ct);
            try
            {
                var bytes = ReadImage(t.ImageId);
                if (bytes == null)
                {
                    TimeSpan wait;
                    lock (added)
                    {
                        var now = DateTime.UtcNow;
                        if (nextStart < now) nextStart = now;
                        wait = nextStart - now;
                        nextStart += TimeSpan.FromMilliseconds(85);
                    }
                    await Task.Delay(wait, ct);
                    bytes = await http.GetByteArrayAsync(string.Format(ImageUrl, t.ImageId), ct);
                    SaveImage(t.ImageId, bytes);
                }
                var px = Pixels.Load(new MemoryStream(bytes));
                var entry = new ArtEntry(t.ImageId, t.Card.Id, t.Card.Name, ArtPrint.Of(px, 0, 0, px.Width, px.Height));
                lock (added) added.Add(entry);
            }
            catch (HttpRequestException) { } // one missing image shouldn't stop the rest; it's retried next time
            finally
            {
                gate.Release();
                progress?.Report((Interlocked.Increment(ref done), todo.Count));
            }
        });
        await Task.WhenAll(tasks);

        Entries.AddRange(added.OrderBy(e => e.ImageId));
        Updated = DateTime.UtcNow;
        Prepare();
        return added.Count;
    }

    static byte[]? ReadImage(int imageId)
    {
        foreach (var dir in DataDirs)
        {
            try { return File.ReadAllBytes(Path.Combine(dir, "art", imageId + ".jpg")); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
        return null;
    }

    static void SaveImage(int imageId, byte[] bytes) => SaveToFirstWritable(Path.Combine("art", imageId + ".jpg"), f => f.Write(bytes));

    /// <summary>Next to the exe if that folder is writable, else in %LOCALAPPDATA%. Silently skipped if neither is.</summary>
    public void Save(string fileName = FileName) => SaveToFirstWritable(fileName, Write);

    internal static void SaveToFirstWritable(string relativePath, Action<Stream> write)
    {
        foreach (var dir in DataDirs)
        {
            var path = Path.Combine(dir, relativePath);
            var tmp = path + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                using (var f = File.Create(tmp)) write(f);
                File.Move(tmp, path, true);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                try { File.Delete(tmp); } catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
            }
        }
    }

    /// <summary>
    /// The best matches for the card at <paramref name="card"/>. The card's edges in a screenshot can be a
    /// pixel or two off, so the top candidates are rescored with the crop nudged around, and then without the
    /// two strips of the print that match worst (see ArtPrint.TrimmedScore).
    /// </summary>
    public List<ArtMatch> Match(Pixels img, Int32Rect card, double visible = 1, int count = 3)
    {
        // A card overlapped by the next one is compared on the columns of its print that can be seen.
        var keep = ArtPrint.VisibleDims(visible);
        (byte[] Print, float[] Norm) Query(double x, double y, double w, double h)
        {
            var print = ArtPrint.Of(img, x, y, w, h);
            return (print, keep == null ? ArtPrint.Normalize(print) : ArtPrint.Normalize(print, keep));
        }
        float Score(float[] q, int i) => keep == null ? Dot(q, _normalized[i]) : ArtPrint.Score(q, Entries[i].Print, keep);

        var q = Query(card.X, card.Y, card.Width, card.Height).Norm;
        var scores = new float[_normalized.Length];
        Parallel.For(0, _normalized.Length, i => scores[i] = Score(q, i));
        // Wide enough that an occluded card's right match still makes it (it can rank ~50th untrimmed).
        var shortlist = Enumerable.Range(0, scores.Length).OrderByDescending(i => scores[i]).Take(60).ToList();

        double step = Math.Max(1, card.Width / 40.0);
        var shifted = new List<(byte[] Print, float[] Norm)>();
        for (int dy = -2; dy <= 2; dy++)
            for (int dx = -2; dx <= 2; dx++)
                for (int ds = -1; ds <= 1; ds++)
                    shifted.Add(Query(card.X + dx * step - ds * step, card.Y + dy * step - ds * step,
                        card.Width + 2 * ds * step, card.Height + 2 * ds * step));

        return shortlist
            .Select(i => new ArtMatch(Entries[i], ArtPrint.TrimmedScore(shifted.MaxBy(s => Score(s.Norm, i)).Print, Entries[i].Print, keep)))
            .OrderByDescending(m => m.Score)
            .DistinctBy(m => m.Entry.CardId)
            .Take(count)
            .ToList();
    }

    static float Dot(float[] a, float[] b)
    {
        var va = System.Runtime.InteropServices.MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(a);
        var vb = System.Runtime.InteropServices.MemoryMarshal.Cast<float, System.Numerics.Vector<float>>(b);
        var acc = System.Numerics.Vector<float>.Zero;
        for (int i = 0; i < va.Length; i++) acc += va[i] * vb[i];
        float sum = System.Numerics.Vector.Dot(acc, System.Numerics.Vector<float>.One);
        for (int i = va.Length * System.Numerics.Vector<float>.Count; i < a.Length; i++) sum += a[i] * b[i];
        return sum;
    }
}

public static class ScreenshotReader
{
    /// <summary>
    /// Below this the art isn't in the indexes (Master Duel has artworks of its own) or the card is newer than
    /// them. On three screenshots the right matches scored 0.84-0.98 (strip-trimmed); cards with no right
    /// answer in the index scored at most 0.64.
    /// </summary>
    public const float Sure = 0.75f;

    /// <summary>
    /// And it must beat the next card by this much. With the right card in the index, it led by 0.109 or more on
    /// three screenshots; without it (Master Duel-only artwork, YGOPRODeck index only) the top two were within
    /// 0.055, both wrong.
    /// </summary>
    public const float Lead = 0.08f;

    /// <summary>
    /// The second pass, for cards the first couldn't place: matched against only the cards already identified
    /// on the same screenshot (decks run several copies), it takes this score with <see cref="DeckLead"/>. That
    /// catches copies Master Duel draws with its shiny foil effect, which shifts their colours: on Charlie's
    /// Ancient Gear screenshot a foil Mulcharmy Purulia scored 0.80 and a foil Ancient Gear Ballista 0.73
    /// there, each leading by 0.2+. Cards whose artwork no index has (MD-only arts, YGOPRODeck index alone)
    /// reached at most 0.57 against their deck's cards, often the right card anyway.
    /// </summary>
    public const float DeckSure = 0.65f, DeckLead = 0.15f;

    /// <summary>
    /// And the deck's card must score within this of the card's best match over all cards. Spatial Trunade
    /// (0.96, tied with its look-alike "Hey, Space Trunade!", neither in the deck) otherwise became the
    /// deck's Terraforming at 0.73. The foils it is for were 0.06 and 0.00 below their best overall.
    /// </summary>
    public const float DeckGap = 0.10f;

    /// <summary>
    /// The third pass, for cards still unplaced: ArtIndex.MatchIgnoringColour over every card. A foil Blackwing
    /// Full Armor Master (one copy, so the second pass can't help) scored 0.722 with a 0.144 lead, and a foil
    /// Ancient Gear Ballista 0.796 / 0.261. Six MD-only artworks with the YGOPRODeck index alone, all wrong,
    /// reached at most 0.580, with leads of at most 0.063.
    /// </summary>
    public const float FoilSure = 0.65f, FoilLead = 0.10f;

    /// <summary>
    /// A deck list for DeckListParser. Sections follow Master Duel's order (Main, Extra, Side); a card in the
    /// wrong one is moved by the usual Extra Deck check. A card that can't be identified is left out of the
    /// deck and written as a # line saying where it is and what it looks most like, so the deck-size check
    /// flags it and a wrong guess never ends up in the code. One placed by the second pass (see DeckSure) is
    /// in the deck, with a # line saying so.
    /// </summary>
    public static (string Text, int Found, int Unsure) Read(Pixels img, IReadOnlyList<ArtIndex> indexes)
    {
        var cards = CardFinder.Find(img);
        static bool Clear(List<ArtMatch> m, float sure, float lead) =>
            m.Count > 0 && m[0].Score >= sure && (m.Count == 1 || m[0].Score - m[1].Score >= lead);

        var matches = cards.Select(c => indexes.SelectMany(ix => ix.Match(img, c.Rect, c.Visible))
            .OrderByDescending(m => m.Score).DistinctBy(m => m.Entry.Name).Take(3).ToList()).ToList();
        var names = new string?[cards.Count];
        for (int i = 0; i < cards.Count; i++)
            if (Clear(matches[i], Sure, Lead)) names[i] = matches[i][0].Entry.Name;

        var fromDeck = new bool[cards.Count];
        var inDeck = names.OfType<string>().ToHashSet();
        if (inDeck.Count > 0 && names.Contains(null))
        {
            var deck = new ArtIndex();
            deck.Entries.AddRange(indexes.SelectMany(ix => ix.Entries).Where(e => inDeck.Contains(e.Name)).DistinctBy(e => (e.ImageId, e.Name)));
            deck.Prepare();
            for (int i = 0; i < cards.Count; i++)
            {
                if (names[i] != null) continue;
                var m = deck.Match(img, cards[i].Rect, cards[i].Visible).DistinctBy(x => x.Entry.Name).ToList();
                // A real foil copy matches every card poorly; one that matched something outside the deck
                // much better is that card (or its look-alike), not a copy of this deck's best fit.
                if (!Clear(m, DeckSure, DeckLead) || matches[i].Count > 0 && m[0].Score < matches[i][0].Score - DeckGap) continue;
                names[i] = m[0].Entry.Name;
                fromDeck[i] = true;
            }
        }

        // Third pass, for what's left: each colour channel on its own, which sees through foil.
        var ignoringColour = new bool[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            if (names[i] != null) continue;
            var m = indexes.SelectMany(ix => ix.MatchIgnoringColour(img, cards[i].Rect, cards[i].Visible))
                .OrderByDescending(x => x.Score).DistinctBy(x => x.Entry.Name).Take(3).ToList();
            if (!Clear(m, FoilSure, FoilLead)) continue;
            names[i] = m[0].Entry.Name;
            ignoringColour[i] = true;
        }

        var sb = new StringBuilder();
        int unsure = 0, start = 0;
        foreach (var section in cards.GroupBy(c => c.Section))
        {
            sb.Append(section.Key switch { 0 => "Main Deck:", 1 => "Extra Deck:", _ => "Side Deck:" }).Append('\n');
            var counts = new List<(string Name, int Count)>();
            var notes = new List<string>();
            int row = 0, col = 0, rowY = section.First().Rect.Y;
            for (int i = start; i < start + section.Count(); i++)
            {
                var rect = cards[i].Rect;
                if (rect.Y > rowY + rect.Height / 2) { row++; col = 0; rowY = rect.Y; }
                col++;
                var where = $"row {row + 1}, card {col}";
                if (names[i] is not { } name)
                {
                    var m = matches[i];
                    // Two good matches too close to call: cards drawn from the same artwork (Crimson Blader and
                    // Crimson Blade Dragon tie at 0.94 / 0.91 when the right edge is overlapped).
                    notes.Add(m.Count > 1 && m[1].Score >= Sure
                        ? $"# ? {where}: either {m[0].Entry.Name} or {m[1].Entry.Name} (their artworks are nearly the same)"
                        : $"# ? {where}: not recognized" +
                          (m.Count > 0 ? $" (looks most like {string.Join(", ", m.Select(x => x.Entry.Name))})" : ""));
                    unsure++;
                    continue;
                }
                if (fromDeck[i]) notes.Add($"# {where}: read as {name} from the other copies on the screenshot (a shiny foil card?); check it");
                if (ignoringColour[i]) notes.Add($"# {where}: read as {name} by shape, ignoring colour (a shiny foil card?); check it");
                int k = counts.FindIndex(c => c.Name == name);
                if (k < 0) counts.Add((name, 1));
                else counts[k] = (name, counts[k].Count + 1);
            }
            start += section.Count();
            foreach (var (name, count) in counts) sb.Append(count).Append(' ').Append(name).Append('\n');
            foreach (var line in notes) sb.Append(line).Append('\n');
            sb.Append('\n');
        }
        return (sb.ToString(), cards.Count, unsure);
    }
}

/// <summary>A card from MDPro's databases, with alternate-art ids already resolved to the real card.</summary>
public sealed record MdProCard(int Id, string Name, int Type);

/// <summary>
/// MDPro (a fan-made Master Duel client) draws many cards with artwork YGOPRODeck doesn't have: anime and
/// Master Duel alternate arts in Picture\Art (art only, named by id), frameless full-card arts in
/// Picture\OverFrame, and pre-release cards as full card pictures in Expansions\*.ypk (zips with pics\ and
/// their own .cdb databases). The ids are often MDPro's own alternate-art ids (Dark Magician 46986414 has
/// art 46986431), which the databases map back to the real card through the alias column.
/// The index is kept as mdpro-index.bin and rebuilt whenever a game update touches any of those.
/// </summary>
public static class MdProArt
{
    const string FileName = "mdpro-index.bin";

    public static bool IsInstall(string root) => Directory.Exists(Path.Combine(root, "Picture", "Art"));

    /// <summary>
    /// Where MDPro is installed, from the log Unity writes on every launch, e.g.
    /// "Discovering subsystems at path F:/Games/MDPro4/MDPro4_Data/UnitySubsystems". Null if it was never run.
    /// </summary>
    public static string? Find()
    {
        var lowDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "YGO");
        foreach (var client in new[] { "MDPro4", "MDPro3" })
            foreach (var log in new[] { "Player.log", "Player-prev.log" })
            {
                try
                {
                    var path = Path.Combine(lowDir, client, log);
                    if (!File.Exists(path)) continue;
                    using var reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    var m = System.Text.RegularExpressions.Regex.Match(reader.ReadToEnd(), @"([A-Za-z]:/[^\r\n']*?)/" + client + "_Data/");
                    if (m.Success && IsInstall(m.Groups[1].Value)) return Path.GetFullPath(m.Groups[1].Value);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
        return null;
    }

    static string[] Packs(string root)
    {
        var dir = Path.Combine(root, "Expansions");
        return Directory.Exists(dir) ? Directory.GetFiles(dir, "*.ypk") : Array.Empty<string>();
    }

    /// <summary>The databases' card list, by image id: cards.cdb, then each pack's (a pack's row wins).</summary>
    public static Dictionary<int, MdProCard> ReadCards(string root)
    {
        var rows = new Dictionary<int, (int Alias, int Type, string Name)>();
        void Add(string cdb)
        {
            foreach (var r in WinSqlite.Query(cdb, "select d.id, d.alias, d.type, t.name from datas d join texts t on t.id = d.id"))
                rows[int.Parse(r[0])] = (int.Parse(r[1]), int.Parse(r[2]), r[3]);
        }
        Add(MainCdb(root));
        foreach (var pack in Packs(root))
        {
            try
            {
                using var zip = ZipFile.OpenRead(pack);
                foreach (var entry in zip.Entries.Where(e => e.FullName.EndsWith(".cdb", StringComparison.OrdinalIgnoreCase)))
                {
                    // SQLite needs a file: copy the database out, read it, delete it.
                    var tmp = Path.Combine(Path.GetTempPath(), $"ydke-{Guid.NewGuid():N}.cdb");
                    try
                    {
                        entry.ExtractToFile(tmp);
                        Add(tmp);
                    }
                    finally { File.Delete(tmp); }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException) { }
        }

        var cards = new Dictionary<int, MdProCard>();
        foreach (var (id, r) in rows)
        {
            int real = r.Alias != 0 && rows.ContainsKey(r.Alias) ? r.Alias : id;
            cards[id] = new MdProCard(real, rows[real].Name, rows[real].Type);
        }
        return cards;
    }

    static List<string> ArtDirs(string root) =>
        new[] { "Art", "OverFrame" }.Select(d => Path.Combine(root, "Picture", d)).Where(Directory.Exists).ToList();

    static string MainCdb(string root) => Path.Combine(root, "Data", "locales", "en-US", "cards.cdb");

    /// <summary>
    /// Changes whenever a game update adds or replaces artwork or card data: that updates the art folder's
    /// timestamp, and "Update Pre-Release Cards" downloads each pack and swaps it in for the old one, so the
    /// .ypk is a new file. cards.cdb counts too, since the index holds names. Stored as the index's Updated.
    /// </summary>
    public static DateTime Stamp(string root) =>
        ArtDirs(root).Select(Directory.GetLastWriteTimeUtc)
            .Concat(Packs(root).Select(File.GetLastWriteTimeUtc))
            .Append(File.GetLastWriteTimeUtc(MainCdb(root)))
            .Max();

    /// <summary>The saved index if nothing has changed since, else a fresh one (about half a minute).</summary>
    public static ArtIndex Load(string root, Dictionary<int, MdProCard> cards, IProgress<(int Done, int Total)>? progress)
    {
        var dirs = ArtDirs(root);
        var packs = Packs(root);
        var stamp = Stamp(root);
        var saved = ArtIndex.ReadSaved(FileName).FirstOrDefault(i => i.Updated == stamp);
        if (saved != null) return saved;

        var sources = new List<(int Id, bool FullCard, Func<byte[]> Bytes)>();
        foreach (var dir in dirs)
            foreach (var file in Directory.EnumerateFiles(dir))
                if (int.TryParse(Path.GetFileNameWithoutExtension(file), out var id))
                    sources.Add((id, dir.EndsWith("OverFrame"), () => File.ReadAllBytes(file)));
        foreach (var pack in packs)
        {
            try
            {
                // pics\<id>.jpg are whole cards; pics\field\ only has field spells' backgrounds.
                using var zip = ZipFile.OpenRead(pack);
                foreach (var entry in zip.Entries)
                {
                    var parts = entry.FullName.Split('/');
                    if (parts.Length != 2 || parts[0] != "pics" || !int.TryParse(Path.GetFileNameWithoutExtension(parts[1]), out var id)) continue;
                    using var s = entry.Open();
                    var bytes = new MemoryStream();
                    s.CopyTo(bytes);
                    sources.Add((id, true, bytes.ToArray));
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException) { }
        }

        var entries = new System.Collections.Concurrent.ConcurrentBag<ArtEntry>();
        int done = 0;
        Parallel.ForEach(sources, t =>
        {
            try
            {
                if (!cards.TryGetValue(t.Id, out var card) || (card.Type & 0x4000) != 0) return; // 0x4000: token, see ArtIndex.UpdateAsync
                var px = Pixels.Load(new MemoryStream(t.Bytes()));
                entries.Add(new ArtEntry(t.Id, card.Id, card.Name, t.FullCard ? ArtPrint.Of(px, 0, 0, px.Width, px.Height) : ArtPrint.OfArtOnly(px)));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or FileFormatException or ArgumentException) { }
            finally
            {
                int n = Interlocked.Increment(ref done);
                if (n % 250 == 0 || n == sources.Count) progress?.Report((n, sources.Count));
            }
        });

        var index = new ArtIndex { Updated = stamp };
        index.Entries.AddRange(entries.OrderBy(e => e.ImageId));
        index.Prepare();
        index.Save(FileName);
        return index;
    }

    /// <summary>
    /// A deck-builder card for one YGOPRODeck doesn't have (pre-release cards). The type is the cdb bitmask:
    /// 1 monster, 2 spell, 4 trap, 0x20 effect, 0x40 fusion, 0x80 ritual, 0x2000 synchro, 0x800000 xyz,
    /// 0x1000000 pendulum, 0x4000000 link.
    /// </summary>
    public static Card ToCard(MdProCard c)
    {
        int t = c.Type;
        if ((t & 2) != 0) return new Card { Id = c.Id, Name = c.Name, Type = "Spell Card", FrameType = "spell" };
        if ((t & 4) != 0) return new Card { Id = c.Id, Name = c.Name, Type = "Trap Card", FrameType = "trap" };
        var frame = (t & 0x40) != 0 ? "fusion" : (t & 0x2000) != 0 ? "synchro" : (t & 0x800000) != 0 ? "xyz"
            : (t & 0x4000000) != 0 ? "link" : (t & 0x80) != 0 ? "ritual" : (t & 0x20) != 0 ? "effect" : "normal";
        bool pendulum = (t & 0x1000000) != 0;
        var type = char.ToUpperInvariant(frame[0]) + frame[1..] + (pendulum ? " Pendulum" : "") + " Monster";
        return new Card { Id = c.Id, Name = c.Name, Type = type, FrameType = pendulum ? frame + "_pendulum" : frame };
    }
}

/// <summary>
/// Card pictures the user drops into my-art\ (next to the exe, or %LOCALAPPDATA%\YdkeDeckBuilder), for cards
/// whose only artwork in the indexes doesn't look like the screenshot. A pre-release card's MDPro picture is
/// often a phone photo of a foil OCG card: on a Blue-Eyes deck screenshot, White-Horned Buster Dragon scored
/// -0.17 against MDPro's (a rainbow foil photo) and 0.865 against a clean picture of the card, leading the
/// next card by 0.25; that picture stayed at or below 0.46 against the other 56 cards on the screen.
/// The file name says which card: "101308022.webp", "101308022 White-Horned Buster Dragon.png" or
/// "White-Horned Buster Dragon.jpg". A whole card or the artwork alone (told apart by shape). Small enough
/// to read again for every screenshot, so a new picture counts at once.
/// </summary>
public static class MyArt
{
    public const string Folder = "my-art";
    static readonly string[] Extensions = { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif" };

    /// <summary>The folder in use, created in the first writable data folder if there's none yet.</summary>
    public static string? Dir()
    {
        var dirs = ArtIndex.DataDirs.Select(d => Path.Combine(d, Folder)).ToList();
        if (dirs.FirstOrDefault(Directory.Exists) is { } found) return found;
        foreach (var dir in dirs)
        {
            try { return Directory.CreateDirectory(dir).FullName; }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
        return null;
    }

    /// <summary>
    /// The pictures as an index, or null if there are none. <paramref name="cards"/> (MDPro's, by id) names a
    /// card given by passcode alone; without it, such a file needs the name after the passcode.
    /// </summary>
    public static ArtIndex? Load(IReadOnlyDictionary<int, MdProCard>? cards)
    {
        if (Dir() is not { } dir) return null;
        var index = new ArtIndex();
        int unnamed = 0;
        foreach (var file in Directory.EnumerateFiles(dir).Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant())))
        {
            var m = System.Text.RegularExpressions.Regex.Match(Path.GetFileNameWithoutExtension(file), @"^\s*(\d{5,9})?[\s_-]*(.*?)\s*$");
            int? id = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : null;
            string? name = m.Groups[2].Value is { Length: > 0 } n ? n
                : id is { } i && cards != null && cards.TryGetValue(i, out var c) ? c.Name : null;
            if (name == null) continue;
            if (id is { } known && cards != null && cards.TryGetValue(known, out var card)) name = card.Name; // MDPro's spelling
            try
            {
                using var s = File.OpenRead(file);
                var px = Pixels.Load(s);
                double aspect = (double)px.Width / px.Height;
                var print = aspect is > 0.6 and < 0.8 ? ArtPrint.Of(px, 0, 0, px.Width, px.Height) : ArtPrint.OfArtOnly(px);
                index.Entries.Add(new ArtEntry(id ?? -++unnamed, id ?? -unnamed, name, print));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or FileFormatException or ArgumentException) { }
        }
        if (index.Entries.Count == 0) return null;
        index.Prepare();
        return index;
    }
}

/// <summary>Read-only queries through the SQLite that ships with Windows 10 and 11, so no package is needed.</summary>
static class WinSqlite
{
    const string Dll = "winsqlite3.dll";
    const int OpenReadOnly = 1, Row = 100;

    [System.Runtime.InteropServices.DllImport(Dll, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    static extern int sqlite3_open_v2(byte[] filename, out IntPtr db, int flags, IntPtr vfs);
    [System.Runtime.InteropServices.DllImport(Dll)] static extern int sqlite3_close(IntPtr db);
    [System.Runtime.InteropServices.DllImport(Dll)] static extern int sqlite3_prepare_v2(IntPtr db, byte[] sql, int bytes, out IntPtr stmt, IntPtr tail);
    [System.Runtime.InteropServices.DllImport(Dll)] static extern int sqlite3_step(IntPtr stmt);
    [System.Runtime.InteropServices.DllImport(Dll)] static extern int sqlite3_column_count(IntPtr stmt);
    [System.Runtime.InteropServices.DllImport(Dll)] static extern IntPtr sqlite3_column_text(IntPtr stmt, int col);
    [System.Runtime.InteropServices.DllImport(Dll)] static extern int sqlite3_finalize(IntPtr stmt);
    [System.Runtime.InteropServices.DllImport(Dll)] static extern IntPtr sqlite3_errmsg(IntPtr db);

    static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s + "\0");

    public static List<string[]> Query(string file, string sql)
    {
        if (sqlite3_open_v2(Utf8(file), out var db, OpenReadOnly, IntPtr.Zero) != 0)
        {
            var msg = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(sqlite3_errmsg(db));
            sqlite3_close(db);
            throw new IOException($"Couldn't open {file}: {msg}");
        }
        try
        {
            if (sqlite3_prepare_v2(db, Utf8(sql), -1, out var stmt, IntPtr.Zero) != 0)
                throw new IOException(System.Runtime.InteropServices.Marshal.PtrToStringUTF8(sqlite3_errmsg(db)));
            try
            {
                var rows = new List<string[]>();
                int cols = sqlite3_column_count(stmt);
                while (sqlite3_step(stmt) == Row)
                {
                    var row = new string[cols];
                    for (int c = 0; c < cols; c++)
                        row[c] = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(sqlite3_column_text(stmt, c)) ?? "";
                    rows.Add(row);
                }
                return rows;
            }
            finally { sqlite3_finalize(stmt); }
        }
        finally { sqlite3_close(db); }
    }
}
