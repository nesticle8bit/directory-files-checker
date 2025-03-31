using System.Security.Cryptography;

namespace DirectoryFilesChecker.Console.Helper
{
    public static class FileCorruptHelper
    {
        public static (bool isCorrupt, string errorMessage) CheckFileIntegrity(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            string[] allowedExtensions = [".txt", ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".xlsx", ".zip", ".gif", ".csv", ".html", ".ppt", ".pptx", ".rar", ".json", ".js", ".css", ".scss", ".map"];

            // Skip unsupported extensions
            if (!allowedExtensions.Contains(extension))
            {
                return (false, "Unsupported file extension");
            }

            try
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    // Read the first few bytes to check file signatures
                    byte[] header = new byte[8]; // Adjust size per file type
                    fs.Read(header, 0, header.Length);

                    switch (extension)
                    {
                        case ".txt":
                        case ".csv":
                        case ".html":
                        case ".json":
                        case ".js":
                        case ".css":
                        case ".scss":
                        case ".map":
                            // Text-based files: No strict signature, just check readability
                            break;

                        case ".pdf":
                            // PDF: Starts with "%PDF-"
                            if (!header.Take(5).SequenceEqual(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }))
                                return (true, "Invalid PDF header");

                            break;

                        case ".jpg":
                        case ".jpeg":
                            // JPEG: Starts with 0xFFD8
                            if (header[0] != 0xFF || header[1] != 0xD8)
                                return (true, "Invalid JPEG header");

                            break;

                        case ".png":
                            // PNG: Starts with 0x89504E470D0A1A0A
                            if (!header.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
                                return (true, "Invalid PNG header");

                            break;

                        case ".gif":
                            // GIF: Starts with "GIF87a" or "GIF89a"
                            if (!(header.Take(3).SequenceEqual(new byte[] { 0x47, 0x49, 0x46 }) && (header[3] == 0x38 || header[3] == 0x39) && header[4] == 0x61))
                                return (true, "Invalid GIF header");

                            break;

                        case ".zip":
                        case ".docx":
                        case ".xlsx":
                        case ".pptx":
                            // ZIP-based formats (DOCX/XLSX/PPTX are ZIP archives)
                            if (header[0] != 0x50 || header[1] != 0x4B) // "PK" (PKZIP)
                                return (true, "Invalid ZIP header");
                            break;

                        case ".rar":
                            // RAR: Starts with "Rar!" (0x52 0x61 0x72 0x21)
                            if (!header.Take(4).SequenceEqual(new byte[] { 0x52, 0x61, 0x72, 0x21 }))
                                return (true, "Invalid RAR header");
                            break;

                        case ".ppt":
                            // PPT (OLE-based): Starts with D0 CF 11 E0 (like .doc/.xls)
                            if (!header.Take(4).SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }))
                                return (true, "Invalid PPT header");
                            break;

                        default:
                            return (false, "Unsupported file extension");
                    }
                }
                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                return (true, ex.Message);
            }
        }

        public static bool HasInvalidCharacters(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return fileName.IndexOfAny(invalidChars) >= 0;
        }

        public static string ComputeSHA256(string filePath)
        {
            try
            {
                using FileStream fs = File.OpenRead(filePath);
                using SHA256 sha256 = SHA256.Create();
                byte[] hashBytes = sha256.ComputeHash(fs);

                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
