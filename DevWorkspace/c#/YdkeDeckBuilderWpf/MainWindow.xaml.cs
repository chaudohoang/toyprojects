using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace YdkeDeckBuilderWpf;

public partial class MainWindow : Window
{
    Deck? _deck;
    Task<ArtIndex>? _art; // loaded on first use: it's a few MB
    // MDPro's card list and artwork, when it's installed, each with the MdProArt.Stamp it was read at.
    (DateTime Stamp, Task<Dictionary<int, MdProCard>> Cards)? _mdproCards;
    (DateTime Stamp, Task<ArtIndex> Art)? _mdproArt;
    readonly AppSettings _settings = AppSettings.Load();
    (Deck Deck, List<Issue> Issues, string? Format)? _shown; // re-rendered when the theme changes

    public MainWindow()
    {
        InitializeComponent();
        if (_settings.MdProFolder == null && MdProArt.Find() is { } found)
        {
            _settings.MdProFolder = found;
            _settings.Save();
        }
        UpdateMdProButton();
        Loaded += (_, _) => Input.Focus();
        SourceInitialized += (_, _) => Theme.ApplyTitleBar(this);
        Theme.Changed += OnThemeChanged;
        Closed += (_, _) => Theme.Changed -= OnThemeChanged;
    }

    void OnThemeChanged()
    {
        if (_shown is { } s) Render(s.Deck, s.Issues, s.Format);
    }

    /// <summary>A palette brush from the active theme (Themes\Light.xaml or Themes\Dark.xaml).</summary>
    Brush Palette(string key) => (Brush)FindResource(key);

    // ---------- Building ----------

    void Build_Click(object sender, RoutedEventArgs e) => _ = BuildAsync();

    void Input_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            e.Handled = true;
            _ = BuildAsync();
        }
    }

    /// <summary>
    /// The text box's Paste command, which Ctrl+V, Shift+Insert and the right-click menu all go through. With a
    /// picture (or a copied image file) and no text on the clipboard it reads the screenshot; the text box
    /// alone would say it can't paste, which greys Paste out in the menu.
    /// </summary>
    void Input_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (e.Command != ApplicationCommands.Paste || !ClipboardImage().HasImage) return;
        e.CanExecute = true;
        e.Handled = true;
    }

    void Input_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command != ApplicationCommands.Paste) return;
        var (hasImage, file) = ClipboardImage();
        if (!hasImage) return;
        e.Handled = true;
        if (file != null) _ = RecognizeFileAsync(file);
        else if (Clipboard.GetImage() is { } image) _ = RecognizeAsync(image);
    }

    /// <summary>
    /// Whether the clipboard holds a picture or a copied image file (File is its path) and no text. Text, or
    /// nothing usable, pastes as usual.
    /// </summary>
    static (bool HasImage, string? File) ClipboardImage()
    {
        try
        {
            if (Clipboard.ContainsText()) return (false, null);
            if (Clipboard.ContainsImage()) return (true, null);
            if (Clipboard.ContainsFileDropList() && ImageFile(Clipboard.GetFileDropList().Cast<string>()) is { } file)
                return (true, file);
        }
        catch (COMException) { } // another app is holding the clipboard
        return (false, null);
    }

    void Input_TextChanged(object sender, TextChangedEventArgs e) =>
        Placeholder.Visibility = Input.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

    async Task BuildAsync()
    {
        if (!BuildButton.IsEnabled) return;
        var entries = DeckListParser.Parse(Input.Text);
        if (entries.Count == 0)
        {
            Status.Text = "Nothing to build: paste a deck list first.";
            return;
        }

        BuildButton.IsEnabled = false;
        Status.Text = $"Looking up {entries.Select(x => x.Name).Distinct().Count()} cards…";
        LookupResult lookup;
        try
        {
            lookup = await CardApi.LookupAsync(entries.Select(x => x.Name));
            await AddMdProCardsAsync(lookup);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            Status.Text = $"Couldn't reach YGOPRODeck ({ex.Message}). Check your internet connection.";
            return;
        }
        finally
        {
            BuildButton.IsEnabled = true;
        }

        var tag = FormatPicker.Children.OfType<RadioButton>().First(r => r.IsChecked == true).Tag as string ?? "";
        var format = tag.Length > 0 ? tag : null;
        var (deck, issues) = DeckBuilder.Build(entries, lookup, format, AutoMoveBox.IsChecked == true);
        Render(deck, issues, format);
    }

    // ---------- Reading a screenshot ----------

    static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".gif" };

    void OpenImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.webp;*.bmp;*.gif|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) == true) _ = RecognizeFileAsync(dialog.FileName);
    }

    static string? ImageFile(IEnumerable<string>? files) =>
        files?.FirstOrDefault(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

    static string? DroppedImage(DragEventArgs e) => ImageFile(e.Data.GetData(DataFormats.FileDrop) as string[]);

    void Window_DragOver(object sender, DragEventArgs e)
    {
        if (DroppedImage(e) == null) return; // let the text box take dropped text as usual
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    void Window_Drop(object sender, DragEventArgs e)
    {
        if (DroppedImage(e) is { } file) _ = RecognizeFileAsync(file);
    }

    async Task RecognizeFileAsync(string file)
    {
        BitmapSource image;
        try
        {
            using var f = File.OpenRead(file);
            image = Pixels.Decode(f);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or FileFormatException)
        {
            Status.Text = $"Couldn't open {Path.GetFileName(file)}: {ex.Message}";
            return;
        }
        await RecognizeAsync(image);
    }

    /// <summary>Turns a deck screenshot into a deck list in the input box, then builds it.</summary>
    async Task RecognizeAsync(BitmapSource image)
    {
        if (!BuildButton.IsEnabled) return;
        BuildButton.IsEnabled = ImageButton.IsEnabled = MdProButton.IsEnabled = false;
        try
        {
            Status.Text = "Reading the screenshot…";
            image.Freeze();
            var art = await (_art ??= Task.Run(ArtIndex.Load));
            if (art.Updated < DateTime.UtcNow.AddDays(-30) || art.Entries.Count == 0)
            {
                var error = await UpdateArtAsync(art);
                if (error != null && art.Entries.Count == 0)
                {
                    Status.Text = $"Couldn't download card artwork from YGOPRODeck ({error}).";
                    return;
                }
            }

            var indexes = new List<ArtIndex> { art };
            if (await LoadMdProArtAsync() is { } mdpro) indexes.Add(mdpro);
            var mdproCards = await LoadMdProCardsAsync();
            if (await Task.Run(() => MyArt.Load(mdproCards)) is { } mine) indexes.Add(mine);

            var pixels = Pixels.From(image);
            Status.Text = "Reading the screenshot…";
            var (text, found, unsure) = await Task.Run(() => ScreenshotReader.Read(pixels, indexes));
            // A card that matches nothing well may be newer than the index: look once a day at most, then read again.
            if (unsure > 0 && art.Updated < DateTime.UtcNow.AddDays(-1) && await UpdateArtAsync(art) == null)
            {
                Status.Text = "Reading the screenshot…";
                (text, found, unsure) = await Task.Run(() => ScreenshotReader.Read(pixels, indexes));
            }
            if (found == 0)
            {
                Status.Text = "Didn't find any cards in that image. It works best on a screenshot of a deck screen (Master Duel and the like), cropped or not.";
                return;
            }
            Input.Text = text;
            Status.Text = unsure == 0
                ? $"Read all {found} cards from the screenshot."
                : $"Read {found - unsure} of {found} cards from the screenshot. {unsure} couldn't be recognized " +
                  "(two cards with the same artwork, a shiny foil card, artwork it doesn't know, a very new card, or something drawn over it): they're the \"# ?\" lines in the list. Type their names in and build again." +
                  $" To have one recognized next time, put a clean picture of it in {MyArt.Dir() ?? MyArt.Folder}, named after the card (e.g. \"White-Horned Buster Dragon.png\")." +
                  (mdproCards == null ? " If the screenshot is from MDPro, point MDPro folder… at your MDPro install to use its artwork too." : "");
            int copies = text.Split('\n').Count(l => l.StartsWith("# row"));
            if (copies > 0)
                Status.Text += copies == 1
                    ? " 1 card was hard to read (probably a shiny foil card) and was matched another way: see its # line and check it."
                    : $" {copies} cards were hard to read (probably shiny foil cards) and were matched another way: see their # lines and check them.";
        }
        finally
        {
            BuildButton.IsEnabled = ImageButton.IsEnabled = MdProButton.IsEnabled = true;
        }
        var note = Status.Text;
        await BuildAsync();
        if (Output.Visibility == Visibility.Visible) Status.Text = note + " " + Status.Text;
    }

    void MdPro_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Choose your MDPro folder (the one with MDPro4.exe or MDPro3.exe in it)" };
        if (_settings.MdProFolder is { } current && Directory.Exists(current)) dialog.InitialDirectory = current;
        if (dialog.ShowDialog(this) != true) return;
        if (!MdProArt.IsInstall(dialog.FolderName))
        {
            Status.Text = $"{dialog.FolderName} doesn't look like an MDPro install: there's no Picture\\Art folder in it.";
            return;
        }
        _settings.MdProFolder = dialog.FolderName;
        _settings.Save();
        _mdproCards = null;
        _mdproArt = null;
        Status.Text = $"Screenshots will also be matched against MDPro's artwork in {dialog.FolderName}.";
        UpdateMdProButton();
    }

    void UpdateMdProButton() =>
        MdProButton.ToolTip = _settings.MdProFolder is { } f
            ? $"Using MDPro's artwork from {f}. Click to change."
            : "If your screenshots come from MDPro, choose its folder so cards with MDPro's own artwork (anime art, full-art frames) are recognized too.";

    /// <summary>
    /// MDPro's card list (under a second to read), or null if no MDPro folder is set (or it's gone).
    /// Re-read after a game update or "Update Pre-Release Cards", even while the app is open.
    /// </summary>
    async Task<Dictionary<int, MdProCard>?> LoadMdProCardsAsync()
    {
        if (_settings.MdProFolder is not { } root || !MdProArt.IsInstall(root)) return null;
        try
        {
            var stamp = await Task.Run(() => MdProArt.Stamp(root));
            if (_mdproCards?.Stamp != stamp) _mdproCards = (stamp, Task.Run(() => MdProArt.ReadCards(root)));
            return await _mdproCards.Value.Cards;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DllNotFoundException)
        {
            _mdproCards = null;
            return null; // YGOPRODeck still works
        }
    }

    /// <summary>
    /// MDPro's artwork index, or null without MDPro. Built once (about half a minute) and again whenever
    /// MDPro's card list changes, since that's what a game update or new pre-release pack does.
    /// </summary>
    async Task<ArtIndex?> LoadMdProArtAsync()
    {
        if (await LoadMdProCardsAsync() is not { } cards || _mdproCards is not { } c) return null;
        var root = _settings.MdProFolder!;
        var progress = new Progress<(int Done, int Total)>(p => Status.Text = $"Indexing MDPro's card artwork (after an MDPro update only): {p.Done} of {p.Total}…");
        try
        {
            if (_mdproArt?.Stamp != c.Stamp) _mdproArt = (c.Stamp, Task.Run(() => MdProArt.Load(root, cards, progress)));
            return await _mdproArt.Value.Art;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _mdproArt = null;
            return null; // the YGOPRODeck index still works
        }
    }

    /// <summary>
    /// With MDPro installed, the deck is for MDPro. For each name:
    /// - only YGOPRODeck has it: YGOPRODeck's id;
    /// - only MDPro has it (or YGOPRODeck only fuzzy-matched it to another card): MDPro's id;
    /// - both have it with the same id: that id;
    /// - both have it with different ids: MDPro's. That's a pre-release card, which has no passcode yet, so
    ///   each gives it a placeholder of its own (Ghostrick Spunkie: 100272049 in MDPro, 100460049 on YGOPRODeck).
    /// MDPro can have several cards for one name once punctuation is ignored ("Hero Flash!!" and
    /// "H.E.R.O. Flash!"): YGOPRODeck's id counts as the same if it's any of them, and otherwise the
    /// exact spelling wins, then a real passcode over a pre-release placeholder (100000000 and up).
    /// </summary>
    async Task AddMdProCardsAsync(LookupResult lookup)
    {
        if (await LoadMdProCardsAsync() is not { } cards) return;
        var byName = cards.Values.DistinctBy(c => c.Id).ToLookup(c => DeckListParser.Norm(c.Name));
        foreach (var name in lookup.Suggestions.Keys.Concat(lookup.Found.Keys).ToList())
        {
            var mdpro = byName[DeckListParser.Norm(name)].ToList();
            if (mdpro.Count == 0) continue; // only YGOPRODeck (or neither)
            lookup.Found.TryGetValue(name, out var found);
            if (found != null && DeckListParser.Norm(found.Name) != DeckListParser.Norm(name)) found = null; // a fuzzy guess
            if (found != null && mdpro.Any(c => c.Id == found.Id)) continue; // same id in both

            var md = mdpro
                .OrderByDescending(c => string.Equals(c.Name, found?.Name ?? name, StringComparison.OrdinalIgnoreCase))
                .ThenBy(c => c.Id >= 100_000_000)
                .First();
            lookup.Suggestions.Remove(name);
            lookup.Found[name] = found == null
                ? MdProArt.ToCard(md) // only MDPro
                : new Card // both, different ids: MDPro's id, YGOPRODeck's details (banlist, link)
                {
                    Id = md.Id, Name = found.Name, Type = found.Type, FrameType = found.FrameType,
                    Url = found.Url, BanlistInfo = found.BanlistInfo,
                };
        }
    }

    /// <summary>Fetches artwork for cards the index doesn't have yet. Returns the error message, or null.</summary>
    async Task<string?> UpdateArtAsync(ArtIndex art)
    {
        try
        {
            Status.Text = "Checking YGOPRODeck for new cards…";
            var progress = new Progress<(int Done, int Total)>(p =>
                Status.Text = $"Downloading artwork for new cards: {p.Done} of {p.Total}…");
            await art.UpdateAsync(CardApi.Http, progress, CancellationToken.None);
            await Task.Run(() => art.Save());
            return null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return ex.Message;
        }
    }

    // ---------- Rendering ----------

    void Render(Deck deck, List<Issue> issues, string? format)
    {
        _deck = deck;
        _shown = (deck, issues, format);
        int errors = issues.Count(i => i.Kind == IssueKind.Error);
        Status.Text = errors > 0
            ? $"Built with {errors} problem{(errors > 1 ? "s" : "")}. The code still contains every card that was found."
            : $"Main {deck.Size(Section.Main)} · Extra {deck.Size(Section.Extra)} · Side {deck.Size(Section.Side)}";
        Output.Visibility = Visibility.Visible;
        YdkeBox.Text = deck.ToYdke();
        Copied.Text = "";

        IssuesList.ItemsSource = issues.Select(i => new IssueView(i.Text, i.Kind switch
        {
            IssueKind.Error => Palette("Err"),
            IssueKind.Warning => Palette("Warn"),
            _ => Palette("Info"),
        })).ToList();

        static int Order(Card c) => c.Type.Contains("Monster") ? 0 : c.Type.Contains("Spell") ? 1 : 2;
        Brush DotFor(Card c) => Palette(c.IsExtraDeck ? "Extra" : Order(c) switch { 0 => "Monster", 1 => "Spell", _ => "Trap" });

        SectionsList.ItemsSource = new[] { (Section.Main, "Main Deck"), (Section.Extra, "Extra Deck"), (Section.Side, "Side Deck") }
            .Where(s => deck[s.Item1].Count > 0)
            .Select(s => new SectionView(s.Item2, $"{deck.Size(s.Item1)} cards",
                deck[s.Item1].OrderBy(l => Order(l.Card)).Select(l =>
                {
                    var ban = l.Card.BanStatus(format);
                    return new CardRow(l.Count, l.Card.Name, l.Card.Url, l.Card.Type, l.Card.Id, DotFor(l.Card),
                        ban ?? "", Palette(ban == "Forbidden" ? "Err" : "Warn"),
                        ban != null ? Visibility.Visible : Visibility.Collapsed);
                }).ToList()))
            .ToList();
    }

    // ---------- Output actions ----------

    void Copy_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(YdkeBox.Text);
            Copied.Text = "Copied.";
        }
        catch (COMException) // another app is holding the clipboard
        {
            YdkeBox.Focus();
            YdkeBox.SelectAll();
            Copied.Text = "Press Ctrl+C to copy.";
        }
    }

    void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_deck == null) return;
        var dialog = new SaveFileDialog { FileName = "deck.ydk", Filter = "YGOPro deck (*.ydk)|*.ydk|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            File.WriteAllText(dialog.FileName, _deck.ToYdk());
            Copied.Text = "Saved " + Path.GetFileName(dialog.FileName) + ".";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, "Couldn't save the file:\n\n" + ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    void OnLink(object sender, RequestNavigateEventArgs e)
    {
        if (e.Uri is { Scheme: "http" or "https" })
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}

public sealed record IssueView(string Text, Brush Brush);

public sealed record SectionView(string Title, string CountText, List<CardRow> Rows);

public sealed record CardRow(int Count, string Name, string? Url, string Type, int Id, Brush Dot,
    string Ban, Brush BanBrush, Visibility BanVisibility);

/// <summary>settings.json, next to the exe like the art indexes (or in %LOCALAPPDATA% if that isn't writable).</summary>
public sealed class AppSettings
{
    const string FileName = "settings.json";

    public string? MdProFolder { get; set; }

    public static AppSettings Load()
    {
        foreach (var dir in ArtIndex.DataDirs)
        {
            try
            {
                var path = Path.Combine(dir, FileName);
                if (File.Exists(path)) return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { }
        }
        return new();
    }

    public void Save() => ArtIndex.SaveToFirstWritable(FileName,
        f => JsonSerializer.Serialize(f, this, new JsonSerializerOptions { WriteIndented = true }));
}
