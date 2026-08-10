using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Fetches the Open Food Facts nightly exports into a local cache directory.
/// Downloads resume where they left off, and a file already matching the published length is left alone.
/// </summary>
/// <remarks>
/// Open Food Facts rate-limits per IP and asks every client to identify itself. Requests are therefore
/// spaced out, carry a descriptive User-Agent, and back off on 429 — a burst of unidentified requests gets
/// throttled within seconds, and repeat offenders can have the IP banned outright.
/// </remarks>
public class OffDownloader(HttpClient httpClient, OffOptions options, ILogger<OffDownloader> logger)
{
    public const string ProductsFileName = "openfoodfacts-products.jsonl.gz";

    private DateTime _lastRequestUtc = DateTime.MinValue;

    public string ProductsPath => Path.Combine(options.DataDir, ProductsFileName);
    public string TaxonomyPath(string taxonomy) => Path.Combine(options.DataDir, "taxonomies", $"{taxonomy}.json");

    public async Task DownloadAllAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(options.DataDir);
        Directory.CreateDirectory(Path.Combine(options.DataDir, "taxonomies"));

        if (options.SkipDownload)
        {
            logger.LogInformation("SkipDownload is set — using the files already in {DataDir}.", options.DataDir);
            if (!File.Exists(ProductsPath))
                throw new FileNotFoundException("SkipDownload is set but the products dump is missing.", ProductsPath);
            return;
        }

        if (options.UserAgent.Contains("example.com", StringComparison.OrdinalIgnoreCase))
            logger.LogWarning("OpenFoodFacts:UserAgent still holds the placeholder contact address. " +
                              "Open Food Facts asks clients to identify themselves — set a real one.");

        foreach (var taxonomy in options.Taxonomies)
        {
            var path = TaxonomyPath(taxonomy);
            if (IsFresh(path))
            {
                logger.LogInformation("{File} is cached and current, skipping.", Path.GetFileName(path));
                continue;
            }

            await DownloadAsync($"{options.BaseUrl}/taxonomies/{taxonomy}.json", path, cancellationToken);
        }

        await DownloadAsync($"{options.BaseUrl}/{ProductsFileName}", ProductsPath, cancellationToken);
    }

    private bool IsFresh(string path)
    {
        if (options.TaxonomyMaxAgeHours <= 0)
            return false;

        var file = new FileInfo(path);
        return file is { Exists: true, Length: > 0 }
               && DateTime.UtcNow - file.LastWriteTimeUtc < TimeSpan.FromHours(options.TaxonomyMaxAgeHours);
    }

    /// <summary>
    /// Downloads one file, resuming an interrupted transfer.
    /// </summary>
    /// <remarks>
    /// A single ranged GET does the work a HEAD-then-GET pair used to: the response status says whether the
    /// local copy is already complete (416), can be resumed (206) or must be restarted (200). That halves
    /// the request count, which is what tipped the rate limiter over on a run of nine files.
    /// </remarks>
    private async Task DownloadAsync(string url, string targetPath, CancellationToken cancellationToken)
    {
        var name = Path.GetFileName(targetPath);
        var existing = new FileInfo(targetPath);
        var offset = existing.Exists ? existing.Length : 0;

        for (var attempt = 1; ; attempt++)
        {
            await ThrottleAsync(cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(options.UserAgent);
            if (offset > 0)
                request.Headers.Range = new RangeHeaderValue(offset, null);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // The server rejects the range when the local file is already the whole thing.
            if (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
            {
                logger.LogInformation("{File} already complete ({Size:N0} bytes), skipping.", name, offset);
                return;
            }

            if (await ShouldRetryAsync(response, name, attempt, cancellationToken))
                continue;

            response.EnsureSuccessStatusCode();

            var resuming = offset > 0 && response.StatusCode == HttpStatusCode.PartialContent;
            if (offset > 0 && !resuming)
                logger.LogWarning("Range request was not honoured, restarting {File} from the beginning.", name);

            var total = resuming
                ? response.Content.Headers.ContentRange?.Length
                : response.Content.Headers.ContentLength;

            if (resuming)
                logger.LogInformation("Resuming {File} at {Offset:N0} of {Total:N0} bytes...", name, offset, total ?? 0);
            else
                logger.LogInformation("Downloading {File} ({Total:N0} bytes)...", name, total ?? 0);

            await CopyToFileAsync(response, targetPath, resuming, resuming ? offset : 0, total, name, cancellationToken);
            return;
        }
    }

    private async Task CopyToFileAsync(
        HttpResponseMessage response,
        string targetPath,
        bool append,
        long written,
        long? total,
        string name,
        CancellationToken cancellationToken)
    {
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var target = new FileStream(
            targetPath,
            append ? FileMode.Append : FileMode.Create,
            FileAccess.Write, FileShare.None, 1 << 20);

        var buffer = new byte[1 << 20];
        var lastReport = DateTime.UtcNow;
        int read;

        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            written += read;

            if (DateTime.UtcNow - lastReport > TimeSpan.FromSeconds(30))
            {
                var percent = total is > 0 ? $" ({written * 100.0 / total.Value:F1}%)" : "";
                logger.LogInformation("  {File}: {Written:N0} bytes{Percent}", name, written, percent);
                lastReport = DateTime.UtcNow;
            }
        }

        logger.LogInformation("{File} complete ({Size:N0} bytes).", name, written);
    }

    /// <summary>
    /// Decides whether a response is worth another attempt, waiting out the server's cool-off first.
    /// </summary>
    private async Task<bool> ShouldRetryAsync(HttpResponseMessage response, string name, int attempt, CancellationToken cancellationToken)
    {
        var transient = response.StatusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.RequestTimeout
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.GatewayTimeout;

        if (!transient)
            return false;

        if (attempt >= options.MaxRetries)
        {
            logger.LogError("{File}: {Status} after {Attempts} attempts, giving up.", name, (int)response.StatusCode, attempt);
            return false;
        }

        // Prefer the server's own instruction; otherwise back off exponentially from the base delay.
        var retryAfter = response.Headers.RetryAfter?.Delta
            ?? (response.Headers.RetryAfter?.Date is { } date ? (TimeSpan?)(date - DateTimeOffset.UtcNow) : null)
            ?? TimeSpan.FromSeconds(Math.Max(options.RequestDelaySeconds, 1) * Math.Pow(2, attempt));

        if (retryAfter < TimeSpan.Zero)
            retryAfter = TimeSpan.FromSeconds(Math.Max(options.RequestDelaySeconds, 1));

        if (retryAfter > TimeSpan.FromMinutes(10))
            retryAfter = TimeSpan.FromMinutes(10);

        logger.LogWarning("{File}: {Status} — waiting {Delay:g} before attempt {Next} of {Max}.",
            name, (int)response.StatusCode, retryAfter, attempt + 1, options.MaxRetries);

        await Task.Delay(retryAfter, cancellationToken);
        return true;
    }

    /// <summary>
    /// Keeps a minimum gap between requests so a run of small taxonomy files does not burst past the limit.
    /// </summary>
    private async Task ThrottleAsync(CancellationToken cancellationToken)
    {
        if (options.RequestDelaySeconds <= 0)
            return;

        var wait = TimeSpan.FromSeconds(options.RequestDelaySeconds) - (DateTime.UtcNow - _lastRequestUtc);
        if (wait > TimeSpan.Zero)
            await Task.Delay(wait, cancellationToken);

        _lastRequestUtc = DateTime.UtcNow;
    }
}
