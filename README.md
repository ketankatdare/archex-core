# ArchEx Core

ArchEx Core is a .NET CLI that uses Amazon Bedrock plus Microsoft Agents/Extensions.AI to analyze a target codebase and produce a structured architecture report.

## What It Does

- Loads runtime configuration from `src/ArchEx.Cli/appsettings.json`.
- Creates a Bedrock chat client and agent session.
- Exposes two local tools to the model:
  - `MapDirectoryStructure`: recursively maps folders and files (excluding common build/dependency noise).
  - `ReadFileContent`: reads bounded text files from the selected workspace with path safety checks.
- Runs an autonomous architecture audit prompt and streams markdown output to the console.

## Project Structure

- `src/ArchEx.Cli/Program.cs`: CLI entry point, config loading, agent setup, and streaming loop.
- `src/ArchEx.Cli/Tools/DiscoveryTools.cs`: project tree discovery tool.
- `src/ArchEx.Cli/Tools/FileAccessTools.cs`: constrained file read tool.
- `src/ArchEx.Cli/appsettings.json`: runtime configuration (safe defaults only).

## Requirements

- .NET SDK 10 (as targeted by `net10.0` in the project file).
- AWS credentials configured on your machine (environment variables, IAM role, or AWS shared config/credential files).
- Access to the Bedrock model configured in `ModelId`.

## Configuration

Default config file:

```json
{
  "AWS": {
    "Region": "us-east-1"
  },
  "ModelId": "amazon.nova-pro-v1:0"
}
```

Recommended local override pattern:

- Keep `appsettings.json` non-sensitive.
- Put personal profile/experimental settings in `src/ArchEx.Cli/appsettings.local.json` (already gitignored).
- Resolve credentials via standard AWS provider chain instead of hardcoding secrets.

## Build And Run

From the repository root:

```bash
dotnet restore
dotnet build src/ArchEx.Cli/ArchEx.Cli.csproj
dotnet run --project src/ArchEx.Cli/ArchEx.Cli.csproj -- <path-to-target-workspace>
```

If no path is passed, the current working directory is analyzed.

## License

See `LICENSE`.
