using System.Collections.ObjectModel;
using System.Windows;

namespace RomHunter;

public partial class MainWindow : Window
{
    public ObservableCollection<RomSearchResult> Results { get; } = new();
    public ObservableCollection<DownloadItem> Queue { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        ResultsGrid.ItemsSource = Results;
        QueueGrid.ItemsSource = Queue;
        LoadDemoData();
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        Results.Clear();
        var term = SearchBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(term)) return;

        Results.Add(new RomSearchResult
        {
            Title = term,
            Platform = "Game Boy",
            Region = "USA",
            Language = "English",
            Source = "Demo / metadata",
            Authorized = true
        });
    }

    private void LoadDemoData()
    {
        Results.Add(new RomSearchResult
        {
            Title = "Starhawk",
            Platform = "Game Boy",
            Region = "USA",
            Language = "English",
            Source = "Demo / metadata",
            Authorized = true
        });
    }
}

public sealed class RomSearchResult
{
    public string Title { get; set; } = "";
    public string Platform { get; set; } = "";
    public string Region { get; set; } = "";
    public string Language { get; set; } = "";
    public string Source { get; set; } = "";
    public bool Authorized { get; set; }
}

public sealed class DownloadItem
{
    public string Title { get; set; } = "";
    public string Status { get; set; } = "Queued";
    public string Progress { get; set; } = "0%";
}
