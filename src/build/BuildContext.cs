using System.Text.Json;
using System.Xml.Linq;
using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Solution;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

public sealed class BuildContext : FrostingContext
{
  public new string Configuration { get; }

  public bool NoRestore { get; }

  public bool NoBuild { get; }

  public string NuGetSource { get; }

  public string? NugetTestToken { get; }

  public string? NugetToken { get; }

  public bool IsLocalBuild { get; }

  public bool IsServerBuild => !IsLocalBuild;

  public DirectoryPath RootDirectory { get; }

  public FilePath SolutionPath { get; }

  public DirectoryPath OutputDirectory { get; }

  private GitVersionInfo? _gitVersion;

  public GitVersionInfo GitVersion => _gitVersion ??= ComputeGitVersion();

  public BuildContext(ICakeContext context)
      : base(context)
  {
    IsLocalBuild = this.BuildSystem().IsLocalBuild;

    Configuration = context.Arguments.HasArgument("configuration")
        ? context.Arguments.GetArgument("configuration")
        : IsLocalBuild ? "Debug" : "Release";

    NoRestore = context.Arguments.HasArgument("no-restore");
    NoBuild = context.Arguments.HasArgument("no-build");

    NuGetSource = Resolve(context, "nuget-source", "NuGetSource")
        ?? "https://api.nuget.org/v3/index.json";

    NugetTestToken = Resolve(context, "nuget-test-token", "NugetTestToken");
    NugetToken = Resolve(context, "nuget-token", "NugetToken") ?? NugetTestToken;

    RootDirectory = FindRootDirectory(context.Environment.WorkingDirectory);
    SolutionPath = RootDirectory.CombineWithFilePath("src/DependencyGraph.slnx");
    OutputDirectory = RootDirectory.Combine("_output");
  }

  /// <summary>
  /// Restores the dotnet local tools declared in .config/dotnet-tools.json.
  /// Runs from the repository root so the manifest is discovered regardless of the build's working directory.
  /// </summary>
  public void RestoreLocalTools()
  {
    var exitCode = this.StartProcess("dotnet", new ProcessSettings
    {
      Arguments = "tool restore",
      WorkingDirectory = RootDirectory,
    });

    if (exitCode != 0)
    {
      throw new CakeException($"dotnet tool restore failed with exit code {exitCode}.");
    }
  }

  /// <summary>
  /// Computes version information by invoking the GitVersion dotnet local tool and parsing its JSON output.
  /// Runs from the repository root, where both the tool manifest and the .git directory live.
  /// </summary>
  private GitVersionInfo ComputeGitVersion()
  {
    RestoreLocalTools();

    var exitCode = this.StartProcess("dotnet", new ProcessSettings
    {
      Arguments = "gitversion /output json",
      WorkingDirectory = RootDirectory,
      RedirectStandardOutput = true,
    }, out var output);

    var json = string.Join(System.Environment.NewLine, output);

    if (exitCode != 0)
    {
      throw new CakeException($"dotnet gitversion failed with exit code {exitCode}.{System.Environment.NewLine}{json}");
    }

    return JsonSerializer.Deserialize<GitVersionInfo>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new CakeException("Failed to parse GitVersion output.");
  }

  /// <summary>
  /// Test projects are discovered the same way the former NUKE build did it:
  /// any project that sets the MSBuild property <c>IsTestProject</c> to <c>true</c>.
  /// </summary>
  public IEnumerable<FilePath> GetTestProjects()
  {
    var solution = this.ParseSolution(SolutionPath);

    foreach (var project in solution.Projects)
    {
      if (!project.Path.FullPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      var document = XDocument.Load(project.Path.FullPath);
      var isTestProject = document.Descendants()
          .Any(e => e.Name.LocalName == "IsTestProject"
                 && string.Equals(e.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase));

      if (isTestProject)
      {
        yield return project.Path;
      }
    }
  }

  private static string? Resolve(ICakeContext context, string argument, string environmentVariable)
  {
    if (context.Arguments.HasArgument(argument))
    {
      return context.Arguments.GetArgument(argument);
    }

    var value = context.Environment.GetEnvironmentVariable(environmentVariable);
    return string.IsNullOrWhiteSpace(value) ? null : value;
  }

  private static DirectoryPath FindRootDirectory(DirectoryPath start)
  {
    var current = new DirectoryInfo(start.FullPath);
    while (current is not null)
    {
      if (Directory.Exists(System.IO.Path.Combine(current.FullName, ".git")))
      {
        return new DirectoryPath(current.FullName);
      }

      current = current.Parent;
    }

    // Fall back to the working directory if no .git folder is found.
    return start;
  }
}

/// <summary>Subset of the GitVersion JSON output consumed by the build.</summary>
public sealed class GitVersionInfo
{
  public string AssemblySemVer { get; init; } = "";

  public string AssemblySemFileVer { get; init; } = "";

  public string FullSemVer { get; init; } = "";

  public string InformationalVersion { get; init; } = "";

  public string Sha { get; init; } = "";

  public string BranchName { get; init; } = "";

  public string MajorMinorPatch { get; init; } = "";

  public string SemVer { get; init; } = "";
}
