# Regira Office.PDF AI Agent Instructions

---

## Module Context

Part of **Regira Office**. For routing and full module overview, see [`office.instructions.md`](./office.instructions.md).

| Namespace | Covers |
|-----------|--------|
| `Regira.Office.PDF` | HTML→PDF, PDF operations (merge/split/extract), printing |

**Related:**
- [Media / Drawing](../../Common.Media/ai/media.instructions.md) — `IImageService` required by `DocNET.PdfManager`; `IImageFile` ↔ PDF conversion
- [IO.Storage](../../Common.IO.Storage/ai/io.storage.instructions.md) — `IMemoryFile` used for PDF input/output

---

## Installation

```xml
<!-- HTML→PDF (recommended — full options support) -->
<PackageReference Include="Regira.Office.PDF.SelectPdf" Version="5.*" />

<!-- HTML→PDF (headless Chromium) -->
<PackageReference Include="Regira.Office.PDF.Puppeteer" Version="5.*" />
<PackageReference Include="Regira.Office.PDF.MsPlaywright" Version="5.*" />

<!-- PDF operations (merge, split, text, images) — recommended -->
<PackageReference Include="Regira.Office.PDF.DocNET" Version="5.*" />

<!-- PDF operations + printing -->
<PackageReference Include="Regira.Office.PDF.Spire" Version="5.*" />

<!-- Print (Windows) -->
<PackageReference Include="Regira.Office.PDF.PDFtoPrinter" Version="5.*" />
<PackageReference Include="Regira.Office.PDF.PockyBum522" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## Backend Comparison

| Package | Backend | HTML→PDF | PDF Ops | Print |
|---------|---------|----------|---------|-------|
| `PDF.SelectPdf` | Select.HtmlToPdf | ✓ full | — | — |
| `PDF.Puppeteer` | PuppeteerSharp | ✓ A4 | — | — |
| `PDF.MsPlaywright` | Microsoft.Playwright | ✓ A4 | — | — |
| `PDF.DocNET` | Docnet.Core | — | merge, split, img↔pdf, text | — |
| `PDF.Spire` | FreeSpire.PDF | — | merge, split, img, text | ✓ |
| `PDF.PDFtoPrinter` | PDFtoPrinter | — | — | ✓ (Win) |
| `PDF.PockyBum522` | SimpleFreePdfPrinter | — | — | ✓ (Win) |

**Recommendations:**
- HTML → PDF: **SelectPdf** (full options, no browser required)
- PDF operations: **DocNET** (merge, split, images, text extraction)
- Printing: **Spire** (operations + print) or **PDFtoPrinter** (print-only, Windows)
- Pixel-perfect CSS: **Puppeteer** or **Playwright** (headless Chromium, A4 fixed)

---

## Interfaces

### `IHtmlToPdfService`

```csharp
Task<IMemoryFile> Create(HtmlInput input, CancellationToken cancellationToken = default);
```

### `IPdfMerger`

```csharp
Task<IMemoryFile?>             Merge(IEnumerable<IMemoryFile> items, CancellationToken cancellationToken = default);
```

### `IPdfSplitter`

```csharp
Task<IEnumerable<IMemoryFile>>  Split(IMemoryFile pdf, IEnumerable<PdfSplitRange> ranges, CancellationToken cancellationToken = default);
Task<int>                       GetPageCount(IMemoryFile pdf, CancellationToken cancellationToken = default);
```

### `IPdfEditor` (extends `IPdfMerger` + `IPdfSplitter`)

```csharp
Task<IMemoryFile?>  RemovePages(IMemoryFile pdf, IEnumerable<int> pages, CancellationToken cancellationToken = default);
```

### `IPdfToImageService` / `IImagesToPdfService`

```csharp
Task<IEnumerable<IImageFile>>  ToImages(IMemoryFile pdf, PdfToImagesOptions? options = null, CancellationToken cancellationToken = default);
Task<IMemoryFile?>             ImagesToPdf(ImagesInput input, CancellationToken cancellationToken = default);
```

### `IPdfToImageAsyncService`

```csharp
IAsyncEnumerable<IImageFile>  ToImagesAsync(IMemoryFile pdf, PdfToImagesOptions? options = null);
```

### `IPdfTextExtractor`

```csharp
Task<string>          GetText(IMemoryFile pdf, CancellationToken cancellationToken = default);
```

### `IPdfTextService` (extends `IPdfTextExtractor`)

```csharp
Task<IList<string>>   GetTextPerPage(IMemoryFile pdf, CancellationToken cancellationToken = default);
Task<IMemoryFile?>    RemoveEmptyPages(IMemoryFile pdf, CancellationToken cancellationToken = default);
```

### `IPdfPrinter`

```csharp
string              DefaultPrinter { get; }
IEnumerable<string> List();
void                Print(PdfPrinterInput input);
```

### `IPdfService`

Composite: `IPdfEditor + IPdfImageService + IPdfTextService`. Implemented by `PDF.DocNET.PdfManager` and `PDF.Spire.PdfManager`.

---

## Models

### `HtmlInput`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `HtmlContent` | `string?` | `null` | HTML to convert |
| `HeaderHtmlContent` | `string?` | `null` | Repeating page header |
| `FooterHtmlContent` | `string?` | `null` | Repeating page footer |
| `HeaderHeight` | `int?` | `null` | Header height in mm |
| `FooterHeight` | `int?` | `null` | Footer height in mm |
| `Format` | `PageSize` | `A4` | Paper size |
| `Orientation` | `PageOrientation` | `Portrait` | Portrait / Landscape |
| `Margins` | `Margins` | `10mm` all | Page margins (in points) |
| `DPI` | `int` | `96` | Render resolution |

### `PdfSplitRange`

| Property | Type | Description |
|----------|------|-------------|
| `Start` | `int` | First page (1-indexed) |
| `End` | `int?` | Last page (`null` = last page of document) |

### `PdfToImagesOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Size` | `ImageSize?` | `1080 × 1920` | Output image dimensions |
| `Format` | `ImageFormat` | `Jpeg` | Output image format |

### `PdfPrinterInput`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PdfFile` | `IMemoryFile` | *(required)* | PDF to print |
| `PrinterName` | `string?` | default printer | Target printer |
| `PageSize` | `PageSize` | `A4` | Paper size |
| `PageOrientation` | `PageOrientation` | `Portrait` | Orientation |

---

## Usage

```csharp
// HTML → PDF (SelectPdf)
IHtmlToPdfService pdf = new Regira.Office.PDF.SelectPdf.PdfManager();
IMemoryFile file = await pdf.Create(new HtmlInput
{
    HtmlContent = "<h1>Invoice</h1>",
    Format      = PageSize.A4,
    Orientation = PageOrientation.Portrait
});

// Merge PDFs (DocNET)
IPdfMerger merger = new Regira.Office.PDF.DocNET.PdfManager(imageService);
IMemoryFile merged = (await merger.Merge([pdf1, pdf2, pdf3]))!;

// Extract text (DocNET)
IPdfTextExtractor extractor = new Regira.Office.PDF.DocNET.PdfManager(imageService);
string text = await extractor.GetText(pdfFile);
```

---
