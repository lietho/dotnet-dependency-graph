// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using System.Diagnostics;

namespace DependencyGraph.App.Commands
{
  public abstract class RestoringCommand : Command
  {
    protected readonly Option<bool?> NoRestoreOption;

    protected RestoringCommand(string name, string? description = null) : base(name, description)
    {
      NoRestoreOption = new Option<bool?>(["--no-restore" ], description: "Do not restore the project.");
      AddOption(NoRestoreOption);
    }

    protected virtual async Task HandleCommand(FileInfo projectOrSulutionFile, bool? noRestore)
    {
      if (noRestore.GetValueOrDefault() == true)
      {
        await Console.Out.WriteLineAsync($"Skipping restore...{Environment.NewLine}{Environment.NewLine}");
        return;
      }

      await RunRestore(projectOrSulutionFile);
    }
  
    private async Task RunRestore(FileInfo projectOrSulutionFile)
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
