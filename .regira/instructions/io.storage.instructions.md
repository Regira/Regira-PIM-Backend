# Regira IO.Storage AI Agent Instructions

---

## IO Abstractions (from `Regira.Common`)

The IO abstraction hierarchy provides the common file contract used throughout IO.Storage, Drawing, and Office.

### Interface hierarchy

```
┌───────────────────┐    ┌───────────────────┐
│ IMemoryBytesFile  │    │ IMemoryStreamFile │
└─────────┬─────────┘    └─────────┬─────────┘
          └────────────┬───────────┘
                       │
                  IMemoryFile
                ┌──────────────┐
                │ INamedFile   │──▶ FileName
                └──────────────┘
                       │
                ┌──────────────┐
                │ IStorageFile │──▶ Identifier, Path, Prefix
                └──────────────┘
                       │
                ┌──────────────┐
                │ IBinaryFile  │
                └──────────────┘
                       │
                ┌──────────────┐
                │  ITextFile   │──▶ Contents
                └──────────────┘
```

### `BinaryFileItem`

The standard concrete implementation of `IBinaryFile`.

```csharp
var file = new BinaryFileItem
{
    FileName    = "invoice.pdf",
    Bytes       = pdfBytes,
    ContentType = "application/pdf"
};
```

Implicit conversions from `byte[]` and `Stream`:

```csharp
BinaryFileItem f1 = pdfBytes;
BinaryFileItem f2 = someStream;
```

### Extension methods — `BinaryFileExtensions`

```csharp
byte[]? bytes  = file.GetBytes();
Stream? stream = file.GetStream();
long    length = file.GetLength();
bool    hasIt  = file.HasContent();

IBinaryFile f = bytes.ToBinaryFile("invoice.pdf");
IBinaryFile f = stream.ToBinaryFile("data.csv");
IBinaryFile f = memoryFile.ToBinaryFile("copy.pdf");
```

### `ContentTypeUtility`

```csharp
string mime = ContentTypeUtility.GetContentType("report.pdf");  // "application/pdf"
string ext  = ContentTypeUtility.GetExtension("image/webp");    // ".webp"
```

### `FileUtility`

```csharp
byte[]  bytes  = FileUtility.GetBytes(stream);
Stream  stream = FileUtility.GetStream(bytes);
string  text   = FileUtility.GetString(bytes, Encoding.UTF8);
string  b64    = FileUtility.GetBase64String(bytes);
byte[]  back   = FileUtility.GetBytesFromString(b64);  // Base64 → bytes
```

---

## Installation

```xml
<!-- Local file system (also ships the shared abstractions) -->
<PackageReference Include="Regira.IO.Storage" Version="5.*" />

<!-- Azure Blob Storage -->
<PackageReference Include="Regira.IO.Storage.Azure" Version="5.*" />

<!-- SSH / SFTP -->
<PackageReference Include="Regira.IO.Storage.SSH" Version="5.*" />

<!-- GitHub (read-only) -->
<PackageReference Include="Regira.IO.Storage.GitHub" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## Key Concept: Identifier vs. Path vs. URI

All `IFileService` methods use **identifiers** — paths relative to the storage root.

```
Root        →  /var/app/storage/
Prefix      →                   invoices/2024/
FileName    →                                 inv-001.pdf
Identifier  →                   invoices/2024/inv-001.pdf   ← use this in all API calls
Path        →  /var/app/storage/invoices/2024/inv-001.pdf
```

*The Path is intended for internal use only. When working with `IFileService`, always use the Identifier — it abstracts away backend differences and ensures portability.*

| Concept | Description |
|---------|-------------|
| `Root` | Backend-specific base address (local path, Azure container URL, SFTP base dir) |
| `Identifier` | Relative key — `Prefix + FileName` — portable across backend swaps |
| `Path` | `Root + Identifier` — full absolute address |

---

## IFileService

All backends implement this single interface.

### Read

```csharp
Task<bool>                Exists(string identifier)
Task<byte[]?>             GetBytes(string identifier)
Task<Stream?>             GetStream(string identifier)
Task<IEnumerable<string>> List(FileSearchObject? so = null)
IAsyncEnumerable<string>  ListAsync(FileSearchObject? so = null)  // NET10+
```

### Write

```csharp
Task<string> Save(string identifier, byte[] bytes,  string? contentType = null)
Task<string> Save(string identifier, Stream stream, string? contentType = null)
Task         Move(string sourceIdentifier, string targetIdentifier)
Task         Delete(string identifier)
```

> `Save` returns the final identifier — it may differ if the backend renames on conflict.

### URI Helpers

```csharp
string  Root { get; }
string  GetAbsoluteUri(string identifier)   // relative → absolute
string  GetIdentifier(string uri)           // absolute → relative
string? GetRelativeFolder(string identifier) // extract parent folder
```

---

## FileSearchObject

Filter parameter for `List()`.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `FolderUri` | `string?` | `null` | Restrict to this folder |
| `Extensions` | `ICollection<string>?` | `null` | Filter by extension — e.g. `[".jpg", ".png"]` |
| `Recursive` | `bool` | `false` | Include subdirectories |
| `Type` | `FileEntryTypes` | `All` | `Files`, `Directories`, or `All` |

```csharp
var images = await storage.List(new FileSearchObject
{
    FolderUri  = "products/",
    Extensions = [".jpg", ".webp"],
    Recursive  = true,
    Type       = FileEntryTypes.Files
});

// Streaming variant (NET10+)
await foreach (var image in storage.ListAsync(new FileSearchObject
{
    FolderUri  = "products/",
    Extensions = [".jpg", ".webp"],
    Recursive  = true,
    Type       = FileEntryTypes.Files
}))
{
    // process image identifier as it arrives
}
```

---

## Implementations

### Local File System — `BinaryFileService`

**Package:** `Regira.IO.Storage`

```csharp
var service = new BinaryFileService(new FileSystemOptions { RootFolder = "/var/app/storage" });
```

**Text files** — wrap any `IFileService` with `DefaultTextFileService`:

```csharp
var text = new DefaultTextFileService(anyFileService, Encoding.UTF8);
string? content = await text.GetContents("config/app.json");
await text.Save("config/app.json", jsonString);
```

---

### Azure Blob Storage — `BinaryBlobService`

**Package:** `Regira.IO.Storage.Azure`

```csharp
var communicator = new AzureCommunicator(new AzureOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=…",
    ContainerName    = "my-container"
});
await communicator.Open();   // idempotent — safe to call multiple times

var service = new BinaryBlobService(communicator);
```

| Option | Type | Description |
|--------|------|-------------|
| `ConnectionString` | `string` | Azure Storage connection string |
| `ContainerName` | `string` | Blob container name |

---

### SSH / SFTP — `SftpService`

**Package:** `Regira.IO.Storage.SSH`

```csharp
var communicator = new SftpCommunicator(new SftpConfig
{
    Host          = "sftp.example.com",
    Port          = 22,
    UserName      = "deploy",
    Password      = "s3cr3t",
    ContainerName = "/home/deploy/files"
});

var service = new SftpService(communicator);
```

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Host` | `string` | *(required)* | SSH server hostname |
| `Port` | `int` | `22` | SSH port |
| `UserName` | `string` | *(required)* | Login username |
| `Password` | `string?` | `null` | Login password |
| `ContainerName` | `string` | `"/"` | Remote base directory |

> `SftpCommunicator` holds a persistent connection. Dispose it on application shutdown.

---

### GitHub — `GitHubService` (read-only)

**Package:** `Regira.IO.Storage.GitHub`

```csharp
var service = new GitHubService(
    new GitHubOptions
    {
        Uri       = "https://api.github.com/repos/owner/repo/contents/",
        Key       = "ghp_xxxxxxxxxxxx",
        UserAgent = "MyApp/1.0"
    },
    jsonSerializer
);
```

| Option | Type | Description |
|--------|------|-------------|
| `Uri` | `string` | GitHub API contents endpoint (must end with `/contents/`) |
| `Key` | `string?` | Personal Access Token |
| `UserAgent` | `string?` | `User-Agent` header — GitHub requires a non-empty value |

**Supported:** `Exists`, `GetBytes`, `GetStream`, `List`, `ListAsync`
**Not supported:** `Save`, `Move`, `Delete` — throws `NotImplementedException`

---

## ZIP / Compression

### `ZipFileService` — browse an archive via IFileService

```csharp
// Open an existing zip
using var zipService = new ZipFileService(new ZipFileCommunicator { SourceFile = existingZip });
var entries = await zipService.List();
var bytes   = await zipService.GetBytes("report.pdf");

// Start a new empty archive
using var newZip = new ZipFileService(new ZipFileCommunicator());
await newZip.Save("data.csv", csvBytes);
```

| `ZipFileCommunicator` | Type | Description |
|-----------------------|------|-------------|
| `SourceFile` | `IMemoryFile?` | Existing zip to open — omit to start empty |
| `Password` | `string?` | Archive password (optional) |

### `ZipBuilder` — create archives

```csharp
IMemoryFile zip = await new ZipBuilder()
    .For([new BinaryFileItem { Name = "report.pdf", Bytes = pdfBytes },
          new BinaryFileItem { Name = "data.csv",   Bytes = csvBytes }])
    .Build();
```

### `ZipUtility` — extension methods

```csharp
IMemoryFile archive        = files.Zip();
IMemoryFile archive        = paths.Zip(baseFolder: "/var/exports");
BinaryFileCollection items = existingZip.Unzip();
string[] extracted         = existingZip.Unzip(targetDirectory: "/tmp/out");
```

---

## Helpers

### `FileProcessor` — recursive processing

```csharp
await new FileProcessor(fileService).ProcessFiles(
    new FileSearchObject { FolderUri = "exports/", Recursive = true },
    async (identifier, svc) => { /* process each file */ }
);
```

### `FileNameHelper` — unique filenames

```csharp
var helper = new FileNameHelper(fileService);
string safe = await helper.NextAvailableFileName("invoices/report.pdf");
// → "invoices/report-(1).pdf" when original already exists
```

Customise: `new FileNameHelper.Options { NumberPattern = " ({0})" }`

### `ExportHelper` — copy between services

```csharp
await new ExportHelper(source, target)
    .Export(new FileSearchObject { FolderUri = "backups/", Recursive = true });
```

### `FileNameUtility` — path helpers

```csharp
FileNameUtility.GetAbsoluteUri("folder/file.txt", root)
FileNameUtility.GetRelativeUri(absolutePath, root)
FileNameUtility.GetCleanFileName("folder/sub/file.txt")  // → "file.txt"
FileNameUtility.Combine("folder", "sub", "file.txt")
FileNameUtility.SanitizeFilename("con.txt")              // avoids Windows reserved names
```

---

## DI Registration

```csharp
// Local file system
services.AddSingleton<IFileService>(_ =>
    new BinaryFileService(new FileSystemOptions { RootFolder = "/var/app/uploads" }));

// Azure Blob — register options and communicator separately; the service calls Open() lazily
services.AddSingleton(new AzureOptions
{
    ConnectionString = configuration["Azure:Storage"],
    ContainerName    = "uploads"
});
services.AddSingleton<AzureCommunicator>();
services.AddSingleton<IFileService, BinaryBlobService>();
```

---

## Backend Comparison

| Backend | Package | Write | Listing | Notes |
|---------|---------|-------|---------|-------|
| `BinaryFileService` | `Regira.IO.Storage` | ✓ | ✓ | Local disk |
| `BinaryBlobService` | `Regira.IO.Storage.Azure` | ✓ | ✓ | Azure Blob |
| `SftpService` | `Regira.IO.Storage.SSH` | ✓ | ✓ | Remote SSH/SFTP |
| `GitHubService` | `Regira.IO.Storage.GitHub` | — | ✓ | Read-only GitHub |
| `ZipFileService` | `Regira.IO.Storage` | ✓ | ✓ | In-memory ZIP archive |
