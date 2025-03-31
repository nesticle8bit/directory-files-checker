# Directory Files Integrity Checker

![.NET Version](https://img.shields.io/badge/.NET-6.0%2B-blue)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A high-performance tool to scan directories and verify file integrity with parallel processing, checksum validation, and corruption detection.

## Features ✨

- **Multi-threaded scanning** (utilizes all CPU cores)
- **Comprehensive file checks**:
  - Basic file accessibility
  - Header validation (for 20+ file types)
  - SHA-256 checksums
  - Invalid filename detection
- **Detailed JSON reporting** with:
  - File metadata (size, timestamps)
  - Corruption status
  - Error messages
- **Real-time progress tracking**
- **Cross-platform** (Windows/Linux/macOS)

### Prerequisites ⚙️
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) or later

### Clone & Run 🏃🏻‍♂️‍➡️
```bash
git clone https://github.com/nesticle8bit/directory-files-checker
cd directory-files-checker
dotnet run --project DirectoryFilesChecker.Console
```

## Sample Output 📊

```json
{
    "name": "example_document.pdf",
    "path": "folder\\example_document.pdf",
    "size": 68750,
    "lastModified": "2020-06-13T22:31:45.0000000Z",
    "revisionDate": "2025-03-31T20:35:26.4908150Z",
    "fileExtension": ".ai",
    "hash": "64f62822ad12670a4fb24fa2faa39cd024dd680f1bab2676f21d978171d142f9",
    "isCorrupt": false,
    "errorMessage": null,
    "hasInvalidChars": false
},
```

## Supported File Types 📄

|Category|Extensions|
|---|---|
|📄 **Documents**|.pdf, .docx, .xlsx, .pptx, .ppt|
|🖼️ **Images**|.jpg, .jpeg, .png, .gif|
|🗄️ **Archives**|.zip, .rar|
|🔗 **Web Files**|.html, .css, .js, .json, .scss, .map|
|🗃️ **Data**|.txt, .csv|

## Performance Metrics ⚡

| Scenario | Files | Time (6-core CPU) |
|---|---|---|
| Small directory | 535 | 0,36s |
| Medium directory | 10,000 | 3.85s |
| Large directory | 100,000 | - |

## Usage 🚀

Basic scan:

```bash
dotnet run -- --directory "C:\MyFiles"
```

Advanced options:

```bash
dotnet run -- \
    --directory "C:\MyFiles" \
    --output "scan_results.json" \
    --threads 8 \
    --skip-hashes
```

| Parameter       | Description                          | Default               |
|-----------------|--------------------------------------|-----------------------|
| `--directory`   | Path to scan (required)              | N/A                   |
| `--output`      | Custom report file path              | Auto-generated name   |
| `--threads`     | Maximum parallel threads to use      | CPU cores - 1         |
| `--skip-hashes` | Skip SHA-256 computation             | `false`               |