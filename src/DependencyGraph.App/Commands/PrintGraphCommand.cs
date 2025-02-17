// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyGraph.Core.Graph;
using Microsoft.Build.Locator;
using NuGet.Common;

namespace DependencyGraph.App.Commands
{
  internal class PrintGraphCommand : Command
  {
    private readonly ILogger _nugetLogger;
    private readonly DependencyGraphFactoryFactory _dependencyGraphFactoryFactory;
    private readonly DependencyGraphVisualizerFactory _dependencyGraphVisualizerFactory;
    private readonly Argument<FileInfo?> _projectOrSolutionFileArgument;
    private readonly Option<bool?> _noRestoreOption;
    private readonly Option<VisualizerType> _visualizerOption;
    private readonly Option<FileInfo?> _outputFileOption;
    private readonly Option<string[]?> _includeOption;
    private readonly Option<string[]?> _excludeOption;
    private readonly Option<int?> _maxDepthOption;

    public PrintGraphCommand(
      ILogger nugetLogger,
      DependencyGraphFactoryFactory dependencyGraphFactoryFactory,
      DependencyGraphVisualizerFactory dependencyGraphVisualizerFactory) : base("print", "Prints the dependency graph for a project or solution.")
    {
      _nugetLogger = nugetLogger;
      _dependencyGraphFactoryFactory = dependencyGraphFactoryFactory;
      _dependencyGraphVisualizerFactory = dependencyGraphVisualizerFactory;

      _projectOrSolutionFileArgument = new Argument<FileInfo?>("project or solution file", "The project or solution file you want to analyze.")
      {
        Arity = ArgumentArity.ZeroOrOne
      };
      AddArgument(_projectOrSolutionFileArgument);

      _visualizerOption = new Option<VisualizerType>(["--visualizer", "-v"], description: "Selects a visualizer for the output.");
      AddOption(_visualizerOption);

      _outputFileOption = new Option<FileInfo?>(["--output", "-o"], "The output file path. Must be specified if the DGML visualizer is used. Overwrites the file if it already exists.");
      AddOption(_outputFileOption);

      _includeOption = new Option<string[]?>(["--include", "-i"], description: "Include dependencies matching one of the filters. Can include the wildcard characters ? and *.")
      {
        AllowMultipleArgumentsPerToken = true
      };
      AddOption(_includeOption);

      _excludeOption = new Option<string[]?>(["--exclude", "-e"], description: "Explicitly exclude dependencies matching one of the filters. Can include the wildcard characters ? and *.")
      {
        AllowMultipleArgumentsPerToken = true
      };
      AddOption(_excludeOption);

      _maxDepthOption = new Option<int?>(["--max-depth", "-d"], description: "The maximum depth if dependencies to retrieve.");
      AddOption(_maxDepthOption);

      _noRestoreOption = new Option<bool?>(["--no-restore"], description: "Do not restore the project.");
      AddOption(_noRestoreOption);

      this.SetHandler(HandleCommand, _projectOrSolutionFileArgument, _visualizerOption, _outputFileOption, _includeOption, _excludeOption, _maxDepthOption, _noRestoreOption);
    }

    private async Task HandleCommand(FileInfo? projectOrSolutionFile, VisualizerType visualizerType, FileInfo? outputFile, string[]? includes, string[]? excludes, int? maxDepth, bool? noRestore)
    {
      projectOrSolutionFile ??= GetSingleProjectFile();

      if (includes == null || includes.Length == 0)
        includes = ["*"];

      if (!projectOrSolutionFile.Exists)
        throw new CommandException($"Could not find project or solution file {projectOrSolutionFile}.");

      if (outputFile == null && visualizerType == VisualizerType.Dgml)
        throw new CommandException("An output file path must be specified when using the DGML visualizer.");

      await CommandHelper.RestoreIfNecessary(projectOrSolutionFile, noRestore);

      IDependencyGraph graph;
      var dependencyGraphFactory = _dependencyGraphFactoryFactory.Create(new()
      {
        Includes = (includes ?? ["*"]).Select(CommandHelper.WildcardToRegex).ToArray(),
        Excludes = (excludes ?? []).Select(CommandHelper.WildcardToRegex).ToArray(),
        MaxDepth = maxDepth
      });

      if (IsSolutionFile(projectOrSolutionFile))
      {
        try
        {
          // call must be done before Microsoft.Build assembly needs to be loaded
          if (!MSBuildLocator.IsRegistered)
            MSBuildLocator.RegisterDefaults();
        }
        catch (Exception ex)
        {
          throw new CommandException($"Could not detect a suitable .NET SDK ({ex.Message}).", ex);
        }

        graph = dependencyGraphFactory.FromSolutionFile(projectOrSolutionFile);
      }
      else
        graph = CommandHelper.CreateGraphForProjectFile(projectOrSolutionFile, dependencyGraphFactory, _nugetLogger);

      await _dependencyGraphVisualizerFactory.Create(visualizerType, outputFile).VisualizeAsync(graph);
    }

    private static FileInfo GetSingleProjectFile() =>
      CommandHelper.GetSingleFile(
        ["*.csproj", "*.vbproj", "*.sln"],
        "Specify a project or solution file. The current working directory does not contain a project or solution file.",
        "Specify which project or solution file to use because the current working directory contains more than one project or solution file.");

    private static bool IsSolutionFile(FileInfo projectOrSolutionFile) => projectOrSolutionFile.Extension.Equals(".sln", StringComparison.OrdinalIgnoreCase);
  }
}
