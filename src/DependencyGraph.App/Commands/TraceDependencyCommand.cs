// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using DependencyGraph.Core.Diagnostic;
using DependencyGraph.Core.Graph;
using NuGet.Common;
using NuGet.Versioning;

namespace DependencyGraph.App.Commands
{
  internal class TraceDependencyCommand : Command
  {
    private readonly ILogger _nugetLogger;
    private readonly DependencyGraphFactoryFactory _dependencyGraphFactoryFactory;
    private readonly Argument<FileInfo?> _projectFileArgument;
    private Argument<string> _dependencyNameArgument;
    private readonly Option<VersionRange?> _versionRangeOption;
    private readonly Option<bool?> _noRestoreOption;

    public TraceDependencyCommand(ILogger nugetLogger, DependencyGraphFactoryFactory dependencyGraphFactoryFactory) : base("trace", "Traces the paths from the root nodes to a specified dependency in the dependency graph.")
    {
      _nugetLogger = nugetLogger;
      _dependencyGraphFactoryFactory = dependencyGraphFactoryFactory;

      _projectFileArgument = new Argument<FileInfo?>("project file", "The project file you want to analyze.")
      {
        Arity = ArgumentArity.ZeroOrOne
      };
      AddArgument(_projectFileArgument);

      _dependencyNameArgument = new Argument<string>("dependency name", "The name of the dependency to be traced. Can include the wildcard characters ? and *.");
      AddArgument(_dependencyNameArgument);

      _versionRangeOption = new Option<VersionRange?>(["--version", "-v"], description: "Specify a version range for the traced dependency (NuGet packages only; see https://learn.microsoft.com/en-us/nuget/concepts/package-versioning?tabs=semver20sort#version-ranges for details).", parseArgument: ParseVersionRange);
      AddOption(_versionRangeOption);

      _noRestoreOption = new Option<bool?>(["--no-restore"], description: "Do not restore the project.");
      AddOption(_noRestoreOption);

      this.SetHandler(HandleCommand, _projectFileArgument, _dependencyNameArgument, _versionRangeOption, _noRestoreOption);
    }

    private static VersionRange? ParseVersionRange(ArgumentResult argumentResult)
    {
      if (!argumentResult.Tokens.Any())
        return null;

      var value = argumentResult.Tokens.Single().Value;
      if (VersionRange.TryParse(value, out var versionRange))
        return versionRange;

      argumentResult.ErrorMessage = $"'{value}' is not a valid NuGet version range.";
      return null;
    }

    private async Task HandleCommand(FileInfo? projectFile, string dependencyNamePattern, VersionRange? versionRange, bool? noRestore)
    {
      projectFile ??= GetSingleProjectFile();

      if (!projectFile.Exists)
        throw new CommandException($"Could not find project file {projectFile}.");

      await CommandHelper.RestoreIfNecessary(projectFile, noRestore);

      var dependencyGraphFactory = _dependencyGraphFactoryFactory.Create(new());
      var graph = CommandHelper.CreateGraphForProjectFile(projectFile, dependencyGraphFactory, _nugetLogger);
      var diagnostic = new TraceDependencyGraphDiagnostic(CommandHelper.WildcardToRegex(dependencyNamePattern), versionRange);
      var paths = diagnostic.Perform(graph).ToList();

      if (paths.Count == 0)
      {
        await Console.Out.WriteLineAsync("The specified dependency was not found in the dependency graph.");
        return;
      }

      await Console.Out.WriteLineAsync($"Found {paths.Count} paths to the specified dependency:");
      foreach (var path in paths)
        await PrintPath(path);
    }

    private static FileInfo GetSingleProjectFile() =>
      CommandHelper.GetSingleFile(
        ["*.csproj", "*.vbproj"],
        "Specify a project file. The current working directory does not contain a project file.",
        "Specify which project file to use because the current working directory contains more than one project or solution file.");

    private static Task PrintPath(IImmutableList<IDependencyGraphNode> path) => Console.Out.WriteLineAsync(string.Join(" -> ", path));
  }
}
