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
    private readonly Argument<FileInfo?> _projectOrSolutionFileArgument;
    private Argument<string> _dependencyNameArgument;
    private readonly Option<VersionRange?> _versionRangeOption;
    private readonly Option<bool?> _noRestoreOption;

    public TraceDependencyCommand(ILogger nugetLogger, DependencyGraphFactoryFactory dependencyGraphFactoryFactory) : base("trace", "Traces the paths from the root nodes to a specified dependency in the dependency graph.")
    {
      _nugetLogger = nugetLogger;
      _dependencyGraphFactoryFactory = dependencyGraphFactoryFactory;

      _projectOrSolutionFileArgument = new Argument<FileInfo?>("project or solution file")
      {
        Description = "The project or solution file you want to analyze.",
        Arity = ArgumentArity.ZeroOrOne
      };
      Arguments.Add(_projectOrSolutionFileArgument);

      _dependencyNameArgument = new Argument<string>("dependency name")
      {
        Description = "The name of the dependency to be traced. Can include the wildcard characters ? and *."
      };
      Arguments.Add(_dependencyNameArgument);

      _versionRangeOption = new Option<VersionRange?>("--version", "-v")
      {
        Description = "Specify a version range for the traced dependency (NuGet packages only; see https://learn.microsoft.com/en-us/nuget/concepts/package-versioning?tabs=semver20sort#version-ranges for details).",
        CustomParser = ParseVersionRange
      };
      Options.Add(_versionRangeOption);

      _noRestoreOption = new Option<bool?>("--no-restore")
      {
        Description = "Do not restore the project."
      };
      Options.Add(_noRestoreOption);

      this.SetAction(HandleCommand);
    }

    private static VersionRange? ParseVersionRange(ArgumentResult argumentResult)
    {
      if (!argumentResult.Tokens.Any())
        return null;

      var value = argumentResult.Tokens.Single().Value;
      if (VersionRange.TryParse(value, out var versionRange))
        return versionRange;

      argumentResult.AddError($"'{value}' is not a valid NuGet version range.");
      return null;
    }

    private async Task HandleCommand(ParseResult parseResult, CancellationToken cancellationToken)
    {
      var projectOrSolutionFile = parseResult.GetValue(_projectOrSolutionFileArgument);
      var dependencyNamePattern = parseResult.GetValue(_dependencyNameArgument)!;
      var versionRange = parseResult.GetValue(_versionRangeOption);
      var noRestore = parseResult.GetValue(_noRestoreOption);

      projectOrSolutionFile ??= GetSingleProjectFile();

      if (!projectOrSolutionFile.Exists)
        throw new CommandException($"Could not find project or solution file {projectOrSolutionFile}.");

      await CommandHelper.RestoreIfNecessary(projectOrSolutionFile, noRestore);

      var dependencyGraphFactory = _dependencyGraphFactoryFactory.Create(new());
      var graph = CommandHelper.CreateGraph(projectOrSolutionFile, dependencyGraphFactory, _nugetLogger);
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
        ["*.csproj", "*.vbproj", "*.sln", "*.slnx"],
        "Specify a project or solution file. The current working directory does not contain a project or solution file.",
        "Specify which project or solution file to use because the current working directory contains more than one project or solution file.");

    private static Task PrintPath(IImmutableList<IDependencyGraphNode> path) => Console.Out.WriteLineAsync(string.Join(" -> ", path));
  }
}
