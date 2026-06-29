// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DependencyGraph.App.Commands;
using DependencyGraph.Core.Diagnostic;
using DependencyGraph.Core.Graph;
using DependencyGraph.Core.Graph.Factory;
using ModelContextProtocol.Server;
using NuGet.Common;
using NuGet.Versioning;

namespace DependencyGraph.App.Mcp
{
  /// <summary>
  /// MCP tools that expose the dependency-graph analysis to AI agents. Each tool analyzes the project or
  /// solution the server was started with; a call can override this by passing an absolute path explicitly.
  /// </summary>
  [McpServerToolType]
  public class DependencyGraphTools
  {
    private readonly McpServerContext _context;

    public DependencyGraphTools(McpServerContext context)
    {
      _context = context;
    }

    [McpServerTool(Name = "get_dependency_graph")]
    [Description("Builds the full dependency graph (NuGet packages and project references, including transitive dependencies) for a .NET project or solution and returns it as a tree.")]
    public async Task<DependencyGraphDto> GetDependencyGraphAsync(
      [Description("Optional wildcard filters (e.g. 'Microsoft.*'); only dependencies matching one of them are included. Defaults to everything.")] string[]? include = null,
      [Description("Optional wildcard filters (e.g. 'System.*'); dependencies matching one of them are excluded.")] string[]? exclude = null,
      [Description("Optional maximum dependency depth to traverse.")] int? maxDepth = null,
      [Description("Exclude dependencies provided by the shared framework (pruned packages).")] bool excludeFrameworkProvided = false,
      [Description("Skip running 'dotnet restore' before analyzing. Set to true only if the project is already restored.")] bool noRestore = false,
      [Description("Absolute path to the .csproj, .vbproj, .sln or .slnx file to analyze. Defaults to the project or solution the server was started with.")] string? projectOrSolutionFile = null)
    {
      var graph = await BuildGraphAsync(projectOrSolutionFile, include, exclude, maxDepth, excludeFrameworkProvided, noRestore);
      return DependencyGraphDtoMapper.ToDto(graph);
    }

    [McpServerTool(Name = "trace_dependency")]
    [Description("Finds every path from the root project(s) to a specific dependency, showing why that dependency is included.")]
    public async Task<TraceResultDto> TraceDependencyAsync(
      [Description("Name of the dependency to trace. Supports the wildcard characters '?' and '*'.")] string dependencyName,
      [Description("Optional NuGet version range to match (e.g. '12.0', '[1.0,2.0)'); applies to NuGet packages only.")] string? versionRange = null,
      [Description("Exclude dependencies provided by the shared framework (pruned packages).")] bool excludeFrameworkProvided = false,
      [Description("Skip running 'dotnet restore' before analyzing. Set to true only if the project is already restored.")] bool noRestore = false,
      [Description("Absolute path to the .csproj, .vbproj, .sln or .slnx file to analyze. Defaults to the project or solution the server was started with.")] string? projectOrSolutionFile = null)
    {
      var graph = await BuildGraphAsync(projectOrSolutionFile, include: null, exclude: null, maxDepth: null, excludeFrameworkProvided, noRestore);

      var parsedVersionRange = ParseVersionRange(versionRange);
      var diagnostic = new TraceDependencyGraphDiagnostic(CommandHelper.WildcardToRegex(dependencyName), parsedVersionRange);
      var paths = diagnostic.Perform(graph).ToList();

      return DependencyGraphDtoMapper.ToTraceResult(paths);
    }

    [McpServerTool(Name = "list_target_frameworks")]
    [Description("Lists the target frameworks (per project) found in a .NET project or solution, e.g. 'MyProject/net10.0'.")]
    public async Task<IReadOnlyList<string>> ListTargetFrameworksAsync(
      [Description("Skip running 'dotnet restore' before analyzing. Set to true only if the project is already restored.")] bool noRestore = false,
      [Description("Absolute path to the .csproj, .vbproj, .sln or .slnx file to analyze. Defaults to the project or solution the server was started with.")] string? projectOrSolutionFile = null)
    {
      var graph = await BuildGraphAsync(projectOrSolutionFile, include: null, exclude: null, maxDepth: null, excludeFrameworkProvided: false, noRestore);

      return graph.AllNodes
        .OfType<TargetFrameworkDependencyGraphNode>()
        .Select(node => node.Name)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
        .ToList();
    }

    [McpServerTool(Name = "list_packages")]
    [Description("Lists every distinct NuGet package (name and resolved version) referenced anywhere in the dependency graph, including transitive packages.")]
    public async Task<IReadOnlyList<PackageDto>> ListPackagesAsync(
      [Description("Exclude dependencies provided by the shared framework (pruned packages).")] bool excludeFrameworkProvided = false,
      [Description("Skip running 'dotnet restore' before analyzing. Set to true only if the project is already restored.")] bool noRestore = false,
      [Description("Absolute path to the .csproj, .vbproj, .sln or .slnx file to analyze. Defaults to the project or solution the server was started with.")] string? projectOrSolutionFile = null)
    {
      var graph = await BuildGraphAsync(projectOrSolutionFile, include: null, exclude: null, maxDepth: null, excludeFrameworkProvided, noRestore);

      return graph.AllNodes
        .OfType<PackageDependencyGraphNode>()
        .Select(node => new PackageDto(node.Name, node.Version.ToString()))
        .Distinct()
        .OrderBy(package => package.Name, StringComparer.OrdinalIgnoreCase)
        .ThenBy(package => package.Version, StringComparer.OrdinalIgnoreCase)
        .ToList();
    }

    [McpServerTool(Name = "find_version_conflicts")]
    [Description("Finds NuGet packages that are resolved to more than one version within the dependency graph, indicating a potential version conflict.")]
    public async Task<IReadOnlyList<VersionConflictDto>> FindVersionConflictsAsync(
      [Description("Skip running 'dotnet restore' before analyzing. Set to true only if the project is already restored.")] bool noRestore = false,
      [Description("Absolute path to the .csproj, .vbproj, .sln or .slnx file to analyze. Defaults to the project or solution the server was started with.")] string? projectOrSolutionFile = null)
    {
      var graph = await BuildGraphAsync(projectOrSolutionFile, include: null, exclude: null, maxDepth: null, excludeFrameworkProvided: false, noRestore);

      return graph.AllNodes
        .OfType<PackageDependencyGraphNode>()
        .GroupBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
        .Select(group => new
        {
          group.Key,
          Versions = group.Select(node => node.Version.ToString()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(version => version, StringComparer.OrdinalIgnoreCase).ToList()
        })
        .Where(package => package.Versions.Count > 1)
        .Select(package => new VersionConflictDto(package.Key, package.Versions))
        .OrderBy(conflict => conflict.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();
    }

    private async Task<IDependencyGraph> BuildGraphAsync(
      string? projectOrSolutionFile,
      string[]? include,
      string[]? exclude,
      int? maxDepth,
      bool excludeFrameworkProvided,
      bool noRestore)
    {
      var file = ResolveFile(projectOrSolutionFile);

      // Restore output (and the "Skipping restore..." message) must go to stderr so it never corrupts the
      // stdio JSON-RPC stream on stdout.
      await CommandHelper.RestoreIfNecessary(file, noRestore, Console.Error);

      var factory = new DependencyGraphFactoryFactory().Create(new DependencyGraphFactoryOptions
      {
        Includes = (include is { Length: > 0 } ? include : ["*"]).Select(CommandHelper.WildcardToRegex).ToArray(),
        Excludes = (exclude ?? []).Select(CommandHelper.WildcardToRegex).ToArray(),
        MaxDepth = maxDepth,
        ExcludeFrameworkProvidedDependencies = excludeFrameworkProvided
      });

      return CommandHelper.CreateGraph(file, factory, NullLogger.Instance);
    }

    private FileInfo ResolveFile(string? projectOrSolutionFile)
    {
      if (string.IsNullOrWhiteSpace(projectOrSolutionFile))
      {
        return _context.DefaultProjectOrSolutionFile
          ?? throw new ArgumentException("No project or solution file was provided and the server has no default. Pass 'projectOrSolutionFile', or start the server with a path / in a directory containing exactly one project or solution file.");
      }

      if (!Path.IsPathRooted(projectOrSolutionFile))
        throw new ArgumentException($"The path '{projectOrSolutionFile}' must be absolute so it can be located regardless of the server's working directory.", nameof(projectOrSolutionFile));

      var file = new FileInfo(projectOrSolutionFile);

      if (!file.Exists)
        throw new FileNotFoundException($"Could not find project or solution file '{projectOrSolutionFile}'.", projectOrSolutionFile);

      return file;
    }

    private static VersionRange? ParseVersionRange(string? versionRange)
    {
      if (string.IsNullOrWhiteSpace(versionRange))
        return null;

      if (VersionRange.TryParse(versionRange, out var parsed))
        return parsed;

      throw new ArgumentException($"'{versionRange}' is not a valid NuGet version range.", nameof(versionRange));
    }
  }
}
