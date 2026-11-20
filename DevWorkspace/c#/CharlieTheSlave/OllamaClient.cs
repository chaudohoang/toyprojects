using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CharlieTheSlave;

/// <summary>Minimal streaming client for a local Ollama server (/api/chat).</summary>
public sealed class OllamaClient
{
    const int MaxHistoryMessages = 20;

    static readonly HttpClient Http = new() { Timeout = Timeout.InfiniteTimeSpan };

    readonly List<ChatMessage> _history = new();

    public string BaseUrl { get; set; }
    public string Model { get; set; }
    public string SystemPrompt { get; set; }

    public OllamaClient(string baseUrl, string model, string systemPrompt)
    {
        BaseUrl = baseUrl.TrimEnd('/');
        Model = model;
        SystemPrompt = systemPrompt;
    }

    public void ClearHistory() => _history.Clear();

    /// <summary>Sends <paramref name="userText"/> with recent history and streams the reply through <paramref name="onToken"/>.</summary>
    public async Task<string> ChatAsync(string userText, Action<string> onToken, CancellationToken ct)
    {
        var messages = new List<ChatMessage> { new("system", SystemPrompt) };
        messages.AddRange(_history);
        messages.Add(new("user", userText));

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/chat")
        {
            Content = JsonContent.Create(new ChatRequest(Model, messages, Stream: true)),
        };
        using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Ollama returned {(int)response.StatusCode}: {body.Trim()}");
        }

        // Ollama streams one JSON object per line.
        var reply = new System.Text.StringBuilder();
        // .NET Framework's ReadLineAsync takes no token, so cancelling disposes the response to unblock it.
        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
        using var cancel = ct.Register(response.Dispose);
        while (await ReadLineAsync(reader, ct) is { } line)
        {
            if (line.Length == 0) continue;
            var chunk = JsonSerializer.Deserialize<ChatChunk>(line);
            if (chunk?.Error is { } error) throw new InvalidOperationException(error);
            if (chunk?.Message?.Content is { Length: > 0 } token)
            {
                reply.Append(token);
                onToken(token);
            }
            if (chunk?.Done == true) break;
        }

        _history.Add(new("user", userText));
        _history.Add(new("assistant", reply.ToString()));
        if (_history.Count > MaxHistoryMessages) _history.RemoveRange(0, _history.Count - MaxHistoryMessages);
        return reply.ToString();
    }

    static async Task<string?> ReadLineAsync(StreamReader reader, CancellationToken ct)
    {
        try { return await reader.ReadLineAsync(); }
        catch (Exception) when (ct.IsCancellationRequested) { throw new OperationCanceledException(ct); }
    }

    record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream);

    record ChatChunk(
        [property: JsonPropertyName("message")] ChatMessage? Message,
        [property: JsonPropertyName("done")] bool Done,
        [property: JsonPropertyName("error")] string? Error);
}
