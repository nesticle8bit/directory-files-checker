using System.Diagnostics;
using System.Text.Json;
using DirectoryFilesChecker.Console.Helper;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var parsedArgs = ParseArguments(args);

            if (string.IsNullOrEmpty(parsedArgs.Directory))
            {
                Console.Write("📂 Enter the directory path to scan: ");
                parsedArgs.Directory = Console.ReadLine()?.Trim() ?? string.Empty;
            }

            if (!Directory.Exists(parsedArgs.Directory))
            {
                Console.WriteLine("❌ Invalid directory path.");
                return;
            }

            string logFilePath = string.IsNullOrEmpty(parsedArgs.Output)
                ? Path.Combine(parsedArgs.Directory, $"file_check_log_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json")
                : parsedArgs.Output;

            await using (var logFile = new StreamWriter(logFilePath, append: false))
            {
                Console.WriteLine($"⏳ Scanning {parsedArgs.Directory}...");
                await ScanDirectoryAsync(parsedArgs, logFile);
            }

            Console.WriteLine($"\n✅ Scan completed in {stopwatch.Elapsed.TotalSeconds:N2}s");
            Console.WriteLine($"📄 Results saved to: {logFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 Critical error: {ex.Message}");
        }
    }

    private static (string Directory, string Output, int Threads, bool SkipHashes) ParseArguments(string[] args)
    {
        var directory = "";
        var output = "";
        var threads = Math.Max(1, Environment.ProcessorCount - 1);
        var skipHashes = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--directory" when i + 1 < args.Length:
                    directory = args[++i];
                    break;
                case "--output" when i + 1 < args.Length:
                    output = args[++i];
                    break;
                case "--threads" when i + 1 < args.Length:
                    if (int.TryParse(args[++i], out int t)) threads = t;
                    break;
                case "--skip-hashes":
                    skipHashes = true;
                    break;
            }
        }

        return (directory, output, threads, skipHashes);
    }

    static async Task ScanDirectoryAsync(
        (string Directory, string Output, int Threads, bool SkipHashes) settings,
        StreamWriter logFile)
    {
        // JSON array start
        await logFile.WriteLineAsync("[");

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = settings.Threads
        };

        var filePaths = Directory.EnumerateFiles(settings.Directory, "*", SearchOption.AllDirectories);
        long entryCounter = 0;
        int filesProcessed = 0;
        int corruptFiles = 0;

        await Parallel.ForEachAsync(filePaths, options, async (filePath, ct) =>
        {
            var result = await ProcessFileAsync(filePath, settings.SkipHashes);
            var (json, isCorrupt) = result;

            await WriteJsonEntryAsync(logFile, json, Interlocked.Increment(ref entryCounter));

            Interlocked.Increment(ref filesProcessed);
            if (isCorrupt) Interlocked.Increment(ref corruptFiles);

            if (filesProcessed % 10 == 0)
                UpdateProgress(filesProcessed, corruptFiles);
        });

        // JSON array end
        await logFile.WriteLineAsync("\n]");
        Console.WriteLine($"\n📊 Total: {filesProcessed} files | ❌ Corrupt: {corruptFiles}");
    }

    private static async Task<(string json, bool isCorrupt)> ProcessFileAsync(string filePath, bool skipHashes)
    {
        var (isCorrupt, errorMessage) = FileCorruptHelper.CheckFileIntegrity(filePath);
        var fileInfo = new FileInfo(filePath);

        var fileStatus = new
        {
            name = fileInfo.Name,
            path = filePath,
            size = fileInfo.Length,
            lastModified = fileInfo.LastWriteTimeUtc.ToString("o"),
            revisionDate = DateTime.UtcNow.ToString("o"),
            fileExtension = fileInfo.Extension.ToLowerInvariant(),
            hash = skipHashes ? null : await ComputeHashAsync(filePath),
            isCorrupt,
            errorMessage = isCorrupt ? errorMessage : null,
            hasInvalidChars = FileCorruptHelper.HasInvalidCharacters(fileInfo.Name)
        };

        string json = JsonSerializer.Serialize(fileStatus);
        return (json, isCorrupt);
    }

    private static async Task<string> ComputeHashAsync(string filePath)
    {
        return await Task.Run(() => FileCorruptHelper.ComputeSHA256(filePath));
    }

    private static readonly SemaphoreSlim _jsonLock = new(1, 1);
    private static async Task WriteJsonEntryAsync(StreamWriter logFile, string json, long entryNumber)
    {
        await _jsonLock.WaitAsync();
        try
        {
            if (entryNumber > 1)
                await logFile.WriteLineAsync(",");

            await logFile.WriteAsync(json);
        }
        finally
        {
            _jsonLock.Release();
        }
    }

    private static void UpdateProgress(int processed, int corrupt)
    {
        Console.Write($"\r🔍 Scanning... {processed} files | ❌ {corrupt} corrupt");
    }
}