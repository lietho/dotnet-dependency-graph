// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using DependencyGraph.App.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DependencyGraph.App.Commands
{
  internal class McpCommand : Command
  {
    private readonly Argument<FileInfo?> _projectOrSolutionFileArgument;

    public McpCommand() : base("mcp", "Starts a Model Context Protocol (MCP) server over stdio that exposes the dependency graph to AI agents.")
    {
      _projectOrSolutionFileArgument = new Argument<FileInfo?>("project or solution file")
      {
        Description = "The project or solution file the server analyzes by default. If omitted, the single project or solution file in the current directory is used (if any). Tool calls can always override this with their own path.",
        Arity = ArgumentArity.ZeroOrOne
      };
      Arguments.Add(_projectOrSolutionFileArgument);

      this.SetAction(HandleCommand);
    }

    private async Task HandleCommand(ParseResult parseResult, CancellationToken cancellationToken)
    {
      var defaultProjectOrSolutionFile = ResolveDefaultFile(parseResult.GetValue(_projectOrSolutionFileArgument));

      var builder = Host.CreateApplicationBuilder();

      // A stdio MCP server reserves stdout exclusively for the JSON-RPC protocol. Remove the default logging
      // providers (which write to stdout) and route all logging to stderr so it cannot corrupt the protocol.
      builder.Logging.ClearProviders();
      builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

      builder.Services.AddSingleton(new McpServerContext(defaultProjectOrSolutionFile));
      builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(typeof(McpCommand).Assembly);

      using var host = builder.Build();

      // Runs until stdin is closed by the client or the command is cancelled (e.g. Ctrl+C).
      await host.RunAsync(cancellationToken);
    }

    /// <summary>
    /// Determines the default project or solution file for the server. An explicitly specified file must exist
    /// (fail fast on misconfiguration). When none is specified, the single project or solution file in the
    /// working directory is used if there is exactly one; otherwise the server starts without a default and
    /// tool calls must supply a path.
    /// </summary>
    private static FileInfo? ResolveDefaultFile(FileInfo? specifiedFile)
    {
      if (specifiedFile != null)
      {
        if (!specifiedFile.Exists)
          throw new CommandException($"Could not find project or solution file {specifiedFile}.");

        return specifiedFile;
      }

      try
      {
        return CommandHelper.GetSingleFile(
          ["*.csproj", "*.vbproj", "*.sln", "*.slnx"],
          "no project or solution file found",
          "more than one project or solution file found");
      }
      catch (CommandException)
      {
        return null;
      }
    }
  }
}
