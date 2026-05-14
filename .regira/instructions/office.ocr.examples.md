# Office.OCR — Example: Scanned Invoice Processing

> Context: An accounting app accepts scanned invoice images uploaded by users and extracts the text content for further parsing.

## Tesseract — extract text from a scanned invoice

```csharp
var ocr = new Regira.Office.OCR.Tesseract.OcrManager(new OcrManager.Options
{
    Language      = "en",
    DataDirectory = "./tessdata"
});

byte[]     imageBytes = await _fileService.GetBytes("scans/invoice-2024-001.jpg") ?? [];
IMemoryFile imgFile   = imageBytes.ToBinaryFile("invoice.jpg");

string? text = await ocr.Read(imgFile);
if (text != null)
    await ParseInvoiceText(text);
```

## Preprocess before OCR (crop + convert to grayscale)

```csharp
public async Task<string?> ReadCroppedRegion(byte[] imageBytes)
{
    using var img     = _imageService.Parse(imageBytes)!;
    using var cropped = _imageService.CropRectangle(img, new ImageEdgeOffset(top: 50, left: 30, bottom: 50, right: 30));
    using var gray    = _imageService.MakeOpaque(cropped);

    return await _ocr.Read(new ImageFile { Bytes = gray.Bytes });
}
```

## PaddleOCR — multilingual receipt scanning (Windows)

```csharp
var ocr = new Regira.Office.OCR.PaddleOCR.OcrManager();

IMemoryFile scan = receiptBytes.ToBinaryFile("receipt.png");
string? text     = await ocr.Read(scan, lang: "en");
```
