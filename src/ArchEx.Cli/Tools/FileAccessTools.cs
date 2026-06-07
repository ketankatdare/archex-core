using System.ComponentModel;

namespace ArchEx.Cli.Tools;

public class FileAccessTools
{
    private readonly string _rootDirectory;
    private const long MaxReadSizeInBytes = 50 * 1024; // 50 KB Safe Limit

    public FileAccessTools(string rootDirectory)
    {
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    [Description("Reads the textual contents of a specific file within the workspace.")]
    public string ReadFileContent([Description("The relative path of the file to read from the workspace root.")] string relativePath)
    {
        try
        {
            string targetPath = Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));

            // Guard rails: Enforce workspace containment boundary
            if (!targetPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return "Error: Security violation. Access denied outside the workspace root.";
            }

            if (!File.Exists(targetPath))
            {
                return $"Error: File not found at path '{relativePath}'.";
            }

            // Guard rails: Verify file size before reading into memory
            var fileInfo = new FileInfo(targetPath);
            if (fileInfo.Length > MaxReadSizeInBytes)
            {
                return $"Error: File size ({fileInfo.Length / 1024} KB) exceeds the safe inspection limit of 50 KB. Use targeted parsing if available.";
            }

            // Return file text content
            return File.ReadAllText(targetPath);
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }
}