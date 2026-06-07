using Amazon.BedrockRuntime;
using ArchEx.Cli.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace ArchEx.Cli;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=================================================");
        Console.WriteLine("🧱 ArchEx Core - AI Architectural Explorer");
        Console.WriteLine("=================================================\n");

        // 1. Establish Configuration Builder Pipeline
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Idiomatically resolve AWS Options from appsettings configuration
        var awsOptions = config.GetAWSOptions();
        string modelId = config["ModelId"] ?? "amazon.nova-pro-v1:0";

        Console.WriteLine($"⚙️ Configuration Loaded via AWS.Extensions:");
        Console.WriteLine($"   AWS Profile:  {awsOptions.Profile ?? "Default Search Chain"}");
        Console.WriteLine($"   AWS Region:   {awsOptions.Region?.SystemName ?? "Unspecified"}");
        Console.WriteLine($"   Target Model: {modelId}\n");

        // 3. Resolve Target Execution Workspace Path
        string targetWorkspace = args.Length > 0 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
        Console.WriteLine($"🔍 Scanning Target Workspace: {targetWorkspace}\n");

        // 4. Create the Service Client securely via the built-in factory pattern
        using var bedrockClient = awsOptions.CreateServiceClient<IAmazonBedrockRuntime>();

        // 5. Bridge Bedrock into Microsoft.Extensions.AI.IChatClient Primitive
        IChatClient chatClient = bedrockClient.AsIChatClient(modelId);

        // 6. Instantiate Local Code Workspace Tool Schemas
        var discoveryTools = new DiscoveryTools();
        var fileAccessTools = new FileAccessTools(targetWorkspace);

        // Explicitly map C# methods into unopinionated AIFunction wrappers for the LLM
        var mapDirectoryFunc = AIFunctionFactory.Create(discoveryTools.MapDirectoryStructure, "MapDirectoryStructure");
        var readFileFunc = AIFunctionFactory.Create(fileAccessTools.ReadFileContent, "ReadFileContent");

        // 7. Assemble the Agent Persona via the official AsAIAgent Extension
        AIAgent architectureAgent = chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                ModelId = modelId,
                Instructions = $@"You are 'ArchEx Core', an elite engineering system architecture analyzer. 
                Your objective is to independently analyze an unfamiliar local code or configuration folder.
                
                You must follow this step-by-step reasoning cycle:
                1. Initial Map: Run 'MapDirectoryStructure' targeting '{targetWorkspace.Replace("\\", "\\\\")}'. 
                   Inspect the returned folder tree structure to determine the type of workspace (e.g., AWS CloudFormation IaC, .NET 10 solution, Python web app).
                2. Target Selection: Find the crucial files (e.g., config files, templates, program entry files). 
                   Do NOT read binary junk or build logs.
                3. Deep Read: Use 'ReadFileContent' to view the text inside those specific critical files. 
                   You can make multiple sequential file reads if dependencies link them together.
                4. Final Report Output: Summarize your deep discovery using structured Markdown. Provide a clear 'Architectural Blueprint' defining components, and a 'Modernization Matrix' showing how to upgrade it to elite standards.",
                Tools = [mapDirectoryFunc, readFileFunc]
            }
        });

        // 8. Let the agent instance safely provision and configure the session
        AgentSession session = await architectureAgent.CreateSessionAsync();

        string userPrompt = "Execute a complete structural audit on the current workspace directory. Map the files out, evaluate the configurations, and document the architecture.";
        Console.WriteLine("🚀 Launching Autonomous Tool Execution Loop. Please wait...\n");

        try
        {
            // 9. Stream output directly to terminal console using the production loop
            await foreach (var update in architectureAgent.RunStreamingAsync(userPrompt, session))
            {
                Console.Write(update);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Execution Error Encountered: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\n\n=================================================");
        Console.WriteLine("🏁 Analysis Completed Successfully.");
        Console.WriteLine("=================================================");
    }
}