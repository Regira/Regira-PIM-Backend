using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Streams products out of the gzipped JSONL dump. The archive is decompressed on the fly and never
/// materialised on disk — the uncompressed dump runs to well over 100 GB.
/// </summary>
public class OffJsonlReader(ILogger<OffJsonlReader> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        // OFF writes numeric fields as bare numbers in some records and as quoted strings in others.
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = false
    };

    public long MalformedLines { get; private set; }

    public async IAsyncEnumerable<OffProduct> ReadAsync(
        string path,
        int? maxProducts = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
        await using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, System.Text.Encoding.UTF8, false, 1 << 20);

        var yielded = 0;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (line.Length == 0)
                continue;

            OffProduct? product;
            try
            {
                product = JsonSerializer.Deserialize<OffProduct>(line, SerializerOptions);
            }
            catch (JsonException)
            {
                // A handful of records in a 4.7M-row community dump carry field types the schema does not
                // expect. Counting them is more useful than aborting the import.
                MalformedLines++;
                if (MalformedLines <= 5)
                    logger.LogWarning("Skipping malformed JSONL record #{Line}.", MalformedLines);
                continue;
            }

            if (product is null)
                continue;

            yield return product;

            if (maxProducts is not null && ++yielded >= maxProducts)
            {
                logger.LogInformation("Reached the configured limit of {Max:N0} products.", maxProducts);
                yield break;
            }
        }
    }
}
