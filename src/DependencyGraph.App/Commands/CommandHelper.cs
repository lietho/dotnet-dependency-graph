// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Text.RegularExpressions;
using DependencyGraph.Core.Graph;
using DependencyGraph.Core.Graph.Factory;
using NuGet.Common;
using NuGet.ProjectModel;

namespace DependencyGraph.App.Commands
{
  internal static class CommandHelper
  {
    internal static FileInfo GetSingleFile(string[] patterns, string noMatchingFilesMessage, string multipleFilesMatchingMessage)
    {
      var allFiles = patterns.SelectMany(pattern => new DirectoryInfo(Environment.CurrentDirectory).EnumerateFiles(pattern)).ToArray();

      if (allFiles.Length > 1)
        throw new CommandException(multipleFilesMatchingMessage);
      else if (allFiles.Length == 0)
        throw new CommandException(noMatchingFilesMessage);
      return allFiles[0];
    }

    internal static async Task RestoreIfNecessary(FileInfo projectOrSolutionFile, bool? noRestore)
    {
      if (noRestore.GetValueOrDefault() == true)
      {
        await Console.Out.WriteLineAsync($"Skipping restore...{Environment.NewLine}{Environment.NewLine}");
        return;
      }

      await RunRestore(projectOrSolutionFile);
    }

    private static async Task RunRestore(FileInfo projectOrSolutionFile)
    {
      var restoreProcess = new Process();

      restoreProcess.StartInfo.FileName = "dotnet";
      restoreProcess.StartInfo.Arguments = $"restore \"{projectOrSolutionFile.FullName}\"";
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

    internal static string WildcardToRegex(string pattern) => $"^{Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".")}$";

    internal static IDependencyGraph CreateGraphForProjectFile(FileInfo projectFile, IDependencyGraphFactory dependencyGraphFactory, ILogger nugetLogger)
    {
      var lockFileInfo = GetLockFileInfo(projectFile.Directory?.EnumerateDirectories("obj").FirstOrDefault(), LockFileFormat.AssetsFileName) ?? throw new CommandException($"Could not find assets file {LockFileFormat.AssetsFileName}. Please build the project first.");

      var lockFileFormat = new LockFileFormat();
      var lockFile = lockFileFormat.Read(lockFileInfo.FullName, nugetLogger);
      return dependencyGraphFactory.FromLockFile(lockFile);
    }

    private static FileInfo? GetLockFileInfo(DirectoryInfo? directory, string assetsFileName) => directory?.GetFiles(assetsFileName)?.FirstOrDefault();
  }
}
