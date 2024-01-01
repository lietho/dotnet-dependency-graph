// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DependencyGraph.Core.Graph.Factory;
using DependencyGraph.Core.Visualizer.Console;
using DependencyGraph.Core.Visualizer.Dgml;
using DependencyGraph.Core.Visualizer;
using NuGet.Common;
using NuGet.ProjectModel;

namespace DependencyGraph.App.Commands
{
  public class DependencyGraphRootCommand : RootCommand
  {
    private readonly ILogger _nugetLogger;
    private readonly IDependencyGraphFactory _dependencyGraphFactory;

    private readonly Argument<FileInfo?> _projectFileArgument;
    private readonly Option<bool?> _noRestoreOption;
    private readonly Option<VisualizerType> _visualizerOption;
    private readonly Option<FileInfo?> _outputFileOption;
    private readonly Option<string[]?> _includeOption;
    private readonly Option<string[]?> _excludeOption;
    private readonly Option<int?> _maxDepthOption;

    public DependencyGraphRootCommand(IEnumerable<Command> commands, ILogger nugetLogger, IDependencyGraphFactory dependencyGraphFactory) : base("Dependency-graph helps you analyze the dependencies of .NET SDK-style projects.")
    {
      _nugetLogger = nugetLogger;
      _dependencyGraphFactory = dependencyGraphFactory;

      // add child commands
      foreach (var command in commands)
        AddCommand(command);

      _projectFileArgument = new Argument<FileInfo?>("project file", "The project file you want to analyze.")
      {
        Arity = ArgumentArity.ZeroOrOne
      };
      AddArgument(_projectFileArgument);

      _visualizerOption = new Option<VisualizerType>(["--visualizer", "-v"], description: "Selects a visualizer for the output.");
      AddOption(_visualizerOption);

      _outputFileOption = new Option<FileInfo?>(["--output", "-o"], "The output file path. Must be specified if the DGML visualizer is used. Overwrites the file if it already exists.");
      AddOption(_outputFileOption);

      _includeOption = new Option<string[]?>(["--include", "-i"], description: "Include dependencies matching one of the filters.")
      {
        AllowMultipleArgumentsPerToken = true
      };
      AddOption(_includeOption);

      _excludeOption = new Option<string[]?>(["--exclude", "-e"], description: "Explicitly exclude dependencies matching one of the filters.")
      {
        AllowMultipleArgumentsPerToken = true
      };
      AddOption(_excludeOption);

      _maxDepthOption = new Option<int?>(["--max-depth", "-d"], description: "The maximum depth if dependencies to retrieve.");
      AddOption(_maxDepthOption);

      _noRestoreOption = new Option<bool?>(["--no-restore"], description: "Do not restore the project.");
      AddOption(_noRestoreOption);

      this.SetHandler(HandleCommand, _projectFileArgument, _visualizerOption, _outputFileOption, _includeOption, _excludeOption, _maxDepthOption, _noRestoreOption);
    }

    private static FileInfo GetSingleProjectFile()
    {
      var projectFilePatterns = new[] { "*.csproj", "*.vbproj" };
      var allFiles = projectFilePatterns.SelectMany(pattern => new DirectoryInfo(".").EnumerateFiles(pattern)).ToArray();

      if (allFiles.Length > 1)
        throw new CommandException("Specify which project file to use because the curent working directory contains more than one project file.");
      else if (allFiles.Length == 0)
        throw new CommandException("Specify a project file. The current working directory does not contain a project file.");
      return allFiles[0];
    }

    private async Task HandleCommand(FileInfo? projectFile, VisualizerType visualizerType, FileInfo? outputFile, string[]? includes, string[]? excludes, int? maxDepth, bool? noRestore)
    {
      projectFile ??= GetSingleProjectFile();

      if (includes == null || includes.Length == 0)
        includes = ["*"];

      if (!projectFile.Exists)
        throw new CommandException($"Could not find project file {projectFile}.");

      if (outputFile == null && visualizerType == VisualizerType.Dgml)
        throw new CommandException("An output file path must be specified when using the DGML visualizer.");

      await RestoreIfNecessary(projectFile, noRestore);

      var lockFileInfo = GetLockFileInfo(projectFile.Directory?.EnumerateDirectories("obj").FirstOrDefault(), LockFileFormat.AssetsFileName) ?? throw new CommandException($"Could not find assets file {LockFileFormat.AssetsFileName}. Please build the project first.");

      var lockFileFormat = new LockFileFormat();
      var lockFile = lockFileFormat.Read(lockFileInfo.FullName, _nugetLogger);
      var graph = _dependencyGraphFactory.FromLockFile(lockFile, (includes ?? ["*"]).Select(WildcardToRegex).ToArray(), (excludes ?? []).Select(WildcardToRegex).ToArray(), maxDepth);
      await GetVisualizer(visualizerType, outputFile).VisualizeAsync(graph);
    }

    public static string WildcardToRegex(string pattern) => $"^{Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".")}$";

    private static FileInfo? GetLockFileInfo(DirectoryInfo? directory, string assetsFileName) => directory?.GetFiles(assetsFileName)?.FirstOrDefault();

    private static IDependencyGraphVisualizer GetVisualizer(VisualizerType visualizerType, FileInfo? outputFile)
    {
      return visualizerType switch
      {
        VisualizerType.Console => new ConsoleDependencyGraphVisualizer(new ConsoleDependencyGraphVisualizerOptions()),
        VisualizerType.Dgml => new DgmlDependencyGraphVisualizer(new DgmlDependencyGraphVisualizerOptions { OutputFilePath = outputFile!.FullName }),
        _ => throw new ArgumentOutOfRangeException(nameof(visualizerType)),
      };
    }

    private static async Task RestoreIfNecessary(FileInfo projectOrSulutionFile, bool? noRestore)
    {
      if (noRestore.GetValueOrDefault() == true)
      {
        await Console.Out.WriteLineAsync($"Skipping restore...{Environment.NewLine}{Environment.NewLine}");
        return;
      }

      await RunRestore(projectOrSulutionFile);
    }

    private static async Task RunRestore(FileInfo projectOrSulutionFile)
    {
      var restoreProcess = new Process();

      restoreProcess.StartInfo.FileName = "dotnet";
      restoreProcess.StartInfo.Arguments = $"restore \"{projectOrSulutionFile.FullName}\"";
      restoreProcess.StartInfo.UseShellExecute = false;
      restoreProcess.StartInfo.RedirectStandardOutput = true;
      restoreProcess.StartInfo.RedirectStandardError = true;
      restoreProcess.OutputDataReceived += (_, args) => Console.WriteLine(args.Data?.Trim());
      restoreProcess.ErrorDataReceived += (_, args) => Console.Error.WriteLine(args.Data?.Trim());
      restoreProcess.Start();
      restoreProcess.BeginOutputReadLine();
      restoreProcess.BeginErrorReadLine();

      await restoreProcess.WaitForExitAsync();

      if (restoreProcess.ExitCode != 0)
        throw new ApplicationException($"Restore failed; Exit code: {restoreProcess.ExitCode}");
    }
  }
}
