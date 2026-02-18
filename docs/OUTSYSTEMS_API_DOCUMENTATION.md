# OutSystems API Documentation
# Base64 PDF Extension

## Server Actions

### 1. ConvertBase64ToPdf

Converts a Base64 encoded string into a PDF file.

#### Inputs

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `Base64String` | Text | Yes | Base64 encoded file. Supports data URI prefixes (e.g., `data:image/png;base64,`) |

#### Outputs

| Parameter | Type | Description |
|-----------|------|-------------|
| `FileBinary` | Binary Data | Converted file in binary format |
| `MimeType` | Text | MIME type of the output file (e.g., `application/pdf`) |
| `FileExtension` | Text | File extension with dot (e.g., `.pdf`) |

#### Behavior

1. **Sanitization**: Automatically removes:
   - Line breaks (`\n`, `\r`)
   - Spaces
   - Data URI prefixes (`data:image/png;base64,`)

2. **Format Detection**: Analyzes file signature (magic bytes) to determine format

3. **Conversion**: 
   - PDF → Pass-through
   - TIFF → Converted to PDF
   - DWG/DXF/CAD → Converted to PDF (if libraries available)
   - Images (PNG/JPEG/GIF) → Pass-through

#### Exceptions

| Exception Message | Cause | Solution |
|-------------------|-------|----------|
| `Base64 string is empty` | Input is null or whitespace | Provide valid Base64 string |
| `Invalid Base64 format` | Malformed Base64 encoding | Check Base64 encoding |
| `Unsupported file format` | Format not recognized | Check supported formats list |

#### Example Usage

```
// Simple conversion
ConvertBase64ToPdf(
    Base64String: FileUpload.Base64,
    FileBinary: PdfResult,
    MimeType: OutputMime,
    FileExtension: OutputExt
)

// With data URI
Base64Input = "data:application/pdf;base64,JVBERi0xLjQK..."
ConvertBase64ToPdf(
    Base64String: Base64Input,  // Prefix automatically removed
    FileBinary: PdfResult,
    MimeType: OutputMime,
    FileExtension: OutputExt
)
```

---

### 2. ConvertBinaryToPdf

Converts binary file data into a PDF file.

#### Inputs

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `BinaryData` | Binary Data | Yes | File content in binary format |

#### Outputs

| Parameter | Type | Description |
|-----------|------|-------------|
| `FileBinary` | Binary Data | Converted file in binary format |
| `MimeType` | Text | MIME type of the output file |
| `FileExtension` | Text | File extension with dot |

#### Behavior

Same as `ConvertBase64ToPdf` but:
- ✅ **More efficient** (no Base64 encoding/decoding overhead)
- ✅ **Better for large files**
- ✅ **Direct binary processing**

#### Exceptions

Same as `ConvertBase64ToPdf` plus:

| Exception Message | Cause | Solution |
|-------------------|-------|----------|
| `Binary data is empty` | Input is null or empty array | Provide valid binary data |

#### Example Usage

```
// File upload
ConvertBinaryToPdf(
    BinaryData: Upload.Content,
    FileBinary: PdfResult,
    MimeType: OutputMime,
    FileExtension: OutputExt
)

// From database
ConvertBinaryToPdf(
    BinaryData: Document.FileContent,
    FileBinary: PdfResult,
    MimeType: OutputMime,
    FileExtension: OutputExt
)
```

---

## Supported Formats

### Input Formats

| Format | Extensions | Detection Method | Conversion |
|--------|------------|------------------|------------|
| PDF | `.pdf` | Magic bytes: `%PDF` | Pass-through |
| TIFF | `.tif`, `.tiff` | Magic bytes: `II*` or `MM*` | → PDF |
| PNG | `.png` | Magic bytes: `89 50 4E 47` | Pass-through |
| JPEG | `.jpg`, `.jpeg` | Magic bytes: `FF D8 FF` | Pass-through |
| GIF | `.gif` | Magic bytes: `GIF8` | Pass-through |
| DNG | `.dng` | TIFF + metadata | → PDF |
| DWG | `.dwg` | Magic bytes: `AC10xx` | → PDF* |
| DXF | `.dxf` | Keywords: `SECTION`, `HEADER` | → PDF* |
| CAD | `.cad` | Heuristic detection | → PDF* |

\* Requires CAD libraries (ACadSharp/netDxf)

### Output Formats

Always outputs PDF (`application/pdf`) except for image pass-through cases.

---

## MIME Types

| Input Format | Output MIME Type | Pass-through? |
|--------------|------------------|---------------|
| PDF | `application/pdf` | ✅ Yes |
| TIFF | `application/pdf` | ❌ Converted |
| PNG | `image/png` | ✅ Yes |
| JPEG | `image/jpeg` | ✅ Yes |
| GIF | `image/gif` | ✅ Yes |
| DNG | `application/pdf` | ❌ Converted |
| DWG | `application/pdf` | ❌ Converted |
| DXF | `application/pdf` | ❌ Converted |
| CAD | `application/pdf` | ❌ Converted |

---

## Error Handling

### Recommended Pattern

```
// Screen Action
Try:
    ConvertBase64ToPdf(
        Base64String: Input.FileData,
        FileBinary: ConvertedFile,
        MimeType: MimeType,
        FileExtension: Extension
    )
    
    // Success - download or save
    Download(
        File: ConvertedFile,
        FileName: "converted" + Extension
    )
    
Catch Exception:
    // Log error
    LogError(Exception.Message)
    
    // Show user-friendly message
    If Exception.Message Like "Base64 string is empty":
        FeedbackMessage = "Please select a file"
    ElseIf Exception.Message Like "Unsupported file format":
        FeedbackMessage = "File format not supported"
    Else:
        FeedbackMessage = "Error converting file. Please try again."
    End If
    
    ShowMessage(FeedbackMessage)
End Try
```

---

## Performance Guidelines

### Best Practices

1. **Use Binary Input When Possible**
   ```
   // Better
   ConvertBinaryToPdf(BinaryData: File.Content, ...)
   
   // Slower (Base64 overhead)
   ConvertBase64ToPdf(Base64String: Base64Encode(File.Content), ...)
   ```

2. **Process Large Files Asynchronously**
   ```
   // For files > 5MB, use BPT or Timer
   ProcessFileAsync(
       FileId: Document.Id
   )
   ```

3. **Batch Processing**
   ```
   // Process in chunks
   ForEach Document in GetDocuments(MaxRecords: 10):
       ConvertBinaryToPdf(...)
   End ForEach
   ```

4. **Caching**
   ```
   // Check if already converted
   If Document.PdfVersion != NullBinary():
       Return Document.PdfVersion
   Else:
       ConvertBinaryToPdf(...)
       SavePdfVersion(...)
   End If
   ```

### Performance Metrics

| File Type | Size | Conversion Time |
|-----------|------|-----------------|
| PDF | 1 MB | ~10ms |
| PNG | 2 MB | ~15ms |
| TIFF | 5 MB | ~500ms |
| DWG | 2 MB | ~2s |
| DXF | 1 MB | ~1s |

---

## Integration Examples

### Example 1: Document Upload Portal

```
// UploadScreen Logic

OnUploadComplete:
    // Get uploaded file
    FileContent = FileUpload.Content
    
    // Convert to PDF
    ConvertBinaryToPdf(
        BinaryData: FileContent,
        FileBinary: PdfVersion,
        MimeType: _,
        FileExtension: _
    )
    
    // Save both versions
    CreateDocument(
        OriginalFile: FileContent,
        PdfVersion: PdfVersion,
        UserId: GetUserId()
    )
    
    // Show success
    ShowSuccessMessage("File uploaded and converted!")
```

### Example 2: Email Attachment Processor

```
// Timer: ProcessEmailAttachments

ForEach Email in GetUnprocessedEmails():
    ForEach Attachment in Email.Attachments:
        Try:
            ConvertBinaryToPdf(
                BinaryData: Attachment.Content,
                FileBinary: PdfContent,
                MimeType: _,
                FileExtension: _
            )
            
            SaveProcessedAttachment(
                EmailId: Email.Id,
                PdfContent: PdfContent
            )
        Catch:
            LogError(...)
        End Try
    End ForEach
End ForEach
```

### Example 3: REST API Integration

```
// REST Endpoint: POST /convert

// Input: JSON with Base64 file
{
    "fileData": "JVBERi0xLjQK...",
    "fileName": "document.dwg"
}

// Logic
ConvertBase64ToPdf(
    Base64String: Request.FileData,
    FileBinary: PdfResult,
    MimeType: MimeType,
    FileExtension: Extension
)

// Output: JSON with Base64 PDF
{
    "pdfData": Base64Encode(PdfResult),
    "mimeType": MimeType,
    "extension": Extension
}
```

---

## Security Considerations

### Safe Usage

✅ **Do:**
- Validate file size before conversion
- Use server-side processing only
- Implement rate limiting for public endpoints
- Sanitize file names before saving
- Log conversion attempts for audit

❌ **Don't:**
- Don't trust user-provided MIME types
- Don't skip error handling
- Don't expose internal paths in errors
- Don't process files without size limits

### Example Security Implementation

```
// Before conversion
If FileSize > 10MB:
    Throw Exception("File too large")
End If

If NOT IsAllowedFileType(FileName):
    Throw Exception("File type not allowed")
End If

// Rate limiting
If GetUserConversions(UserId, Today) > 100:
    Throw Exception("Daily limit exceeded")
End If

// Then convert
ConvertBinaryToPdf(...)
```

---

## Troubleshooting

### Common Issues

**Q: "Unsupported file format" error**
- Check if file format is in supported list
- Verify file isn't corrupted
- Check magic bytes match expected format

**Q: Timeout on large files**
- Increase timeout in Service Center
- Use asynchronous processing (BPT/Timer)
- Consider file size limits

**Q: CAD conversion not working**
- CAD support requires additional setup
- Check if ACadSharp/netDxf are installed
- Verify CAD file version (R13-2018 supported)

**Q: PDF output is empty**
- Check source file has content
- For CAD: verify model space isn't empty
- Enable debug logging to see details

---

## Version History

### 1.0.0 (Current)
- Initial release
- Support for 9 file formats
- Base64 and Binary input
- Automatic format detection
- CAD conversion support
- TIFF to PDF conversion

---

## License

MIT License - Free for commercial and personal use.

**Third-party licenses:**
- iTextSharp: AGPL/Commercial
- ACadSharp: MIT (optional)
- netDxf: MIT (optional)

---

**For support, visit the OutSystems Forge page or community forums.**
