// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using System.Text.RegularExpressions;
using DependencyGraph.Core.Graph.Factory;
using DependencyGraph.Core.Visualizer;
using DependencyGraph.Core.Visualizer.Console;
using DependencyGraph.Core.Visualizer.Dgml;
using NuGet.Common;
using NuGet.ProjectModel;

namespace DependencyGraph.App.Commands
{
  public class ProjectFileDependenciesCommand : Command
  {
    private readonly Argument<FileInfo?> _projectFileArgument;
    private readonly ILogger _nugetLogger;
    private readonly IDependencyGraphFactory _dependencyGraphFactory;
    private readonly Option<VisualizerType> _visualizerOption;
    private readonly Option<FileInfo?> _outputFileOption;
    private readonly Option<string[]> _includeOption;
    private Option<string[]> _excludeOption;
    private Option<int?> _maxDepthOption;

    public ProjectFileDependenciesCommand(ILogger nugetLogger, IDependencyGraphFactory dependencyGraphFactory) : base("project", "Shows the dependencies of a project file.")
    {
      _nugetLogger = nugetLogger;
      _dependencyGraphFactory = dependencyGraphFactory;

      _projectFileArgument = new Argument<FileInfo?>("project file", GetDefault, "The project file you want to analyze.");
      AddArgument(_projectFileArgument);

      _visualizerOption = new Option<VisualizerType>(new string[] { "--visualizer", "-v" }, description: "Selects a visualizer for the output.");
      AddOption(_visualizerOption);

      _outputFileOption = new Option<FileInfo?>(new[] { "--output", "-o" }, "The output file path. Must be specified if the DGML visualizer is used. Overwrites the file if it already exists.");
      AddOption(_outputFileOption);

      _includeOption = new Option<string[]>(new string[] { "--include", "-i" }, description: "Include dependencies matching one of the filters.", getDefaultValue: () => new[] { "*" })
      {
        AllowMultipleArgumentsPerToken = true
      };
      AddOption(_includeOption);

      _excludeOption = new Option<string[]>(new string[] { "--exclude", "-e" }, description: "Explicitly exclude dependencies matching one of the filters.", getDefaultValue: () => new string[0])
      {
        AllowMultipleArgumentsPerToken = true
      };
      AddOption(_excludeOption);

      _maxDepthOption = new Option<int?>(new string[] { "--max-depth", "-d" }, description: "The maximum depth if dependencies to retrieve.");
      AddOption(_maxDepthOption);

      this.SetHandler(HandleCommand, _projectFileArgument, _visualizerOption, _outputFileOption, _includeOption, _excludeOption, _maxDepthOption);
    }

    private static FileInfo? GetDefault()
    {
      var projectFilePatterns = new[] { "*.csproj", "*.vbproj" };
      return projectFilePatterns.SelectMany(pattern => new DirectoryInfo(".").EnumerateFiles(pattern)).FirstOrDefault();
    }

    private async Task HandleCommand(FileInfo? projectFile, VisualizerType visualizerType, FileInfo? outputFile, string[] includes, string[] excludes, int? maxDepth)
    {
      projectFile ??= GetDefault();

      if (projectFile == null)
        throw new CommandException("Could not find a project file in the current directory.");

      if (!projectFile.Exists)
        throw new CommandException($"Could not find project file {projectFile}.");

      if (outputFile == null && visualizerType == VisualizerType.Dgml)
        throw new CommandException("An output file path must be specified when using the DGML visualizer.");

      var lockFileInfo = GetLockFileInfo(projectFile.Directory?.EnumerateDirectories("obj").FirstOrDefault(), LockFileFormat.AssetsFileName) ?? throw new CommandException($"Could not find assets file {LockFileFormat.AssetsFileName}. Please build the project first.");

      var lockFileFormat = new LockFileFormat();
      var lockFile = lockFileFormat.Read(lockFileInfo.FullName, _nugetLogger);
      var graph = _dependencyGraphFactory.FromLockFile(lockFile, includes.Select(WildcardToRegex).ToArray(), excludes.Select(WildcardToRegex).ToArray(), maxDepth);
      await GetVisualizer(visualizerType, outputFile).VisualizeAsync(graph);
    }

    public static string WildcardToRegex(string pattern) => $"^{Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".")}$";

    private static FileInfo? GetLockFileInfo(DirectoryInfo? directory, string assetsFileName) => directory?.GetFiles(assetsFileName)?.FirstOrDefault();

    private static IDependencyGraphVisualizer GetVisualizer(VisualizerType visualizerType, FileInfo? outputFile)
    {
      switch (visualizerType)
      {
        case VisualizerType.Console:
          return new ConsoleDependencyGraphVisualizer(new ConsoleDependencyGraphVisualizerOptions());

        case VisualizerType.Dgml:
          return new DgmlDependencyGraphVisualizer(new DgmlDependencyGraphVisualizerOptions { OutputFilePath = outputFile!.FullName });

        default:
          throw new ArgumentOutOfRangeException(nameof(visualizerType));
      }
    }
  }
}
