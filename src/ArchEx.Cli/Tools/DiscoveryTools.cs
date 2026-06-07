using System.ComponentModel;
using System.Text;

namespace ArchEx.Cli.Tools;

public class DiscoveryTools
{
    [Description("Recursively scans the local folder path, ignoring deep compiler junk like bin, obj, and .git, and maps a readable file layout structure to determine the project type.")]
    public string MapDirectoryStructure(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return "{\"error\": \"Target directory path does not exist or is inaccessible.\"}";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"--- Directory Layout for: {Path.GetFileName(folderPath)} ---");

        try
        {
            TraverseDirectory(new DirectoryInfo(folderPath), sb, "");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Failed during folder crawl: {ex.Message}\"}}";;
        }
    }

    private void TraverseDirectory(DirectoryInfo currentDir, StringBuilder sb, string indent)
    {
        // Explicitly filter out resource-heavy dependency noise
        if (currentDir.Name is "bin" or "obj" or ".git" or "node_modules" or ".vs")
        {
            return;
        }

        sb.AppendLine($"{indent}📁 {currentDir.Name}/");

        // Map out the individual files inside this folder branch
        foreach (var file in currentDir.GetFiles())
        {
            sb.AppendLine($"{indent}  📄 {file.Name} ({file.Extension})");
        }

        // Drill down recursively into subdirectories
        foreach (var subDir in currentDir.GetDirectories())
        {
            TraverseDirectory(subDir, sb, indent + "    ");
        }
    }
}
