﻿using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Serilog;

#pragma warning disable IDE0051 // Remove unused private members

[DotNetVerbosityMapping]
[GitHubActions(
  "CI",
  GitHubActionsImage.WindowsLatest,
  FetchDepth = 0,
  OnPushBranches = ["main"],
  OnPullRequestBranches = ["main"],
  InvokedTargets = [nameof(Test)],
  ImportSecrets = [nameof(NugetTestToken), nameof(NugetToken)])]
internal class Build : NukeBuild
{
  [Solution(GenerateProjects = true)]
  private readonly Solution Solution;

  [GitVersion]
  private readonly GitVersion GitVersion;

  [GitRepository]
  private readonly GitRepository GitRepository;

  private readonly AbsolutePath OutputDirectory = (RootDirectory / "_output").CreateOrCleanDirectory();

  public static int Main() => Execute<Build>(x => x.Compile);

  [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
  private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  [Parameter("Do not restore the solution before building.")]
  private readonly bool NoRestore = false;

  [Parameter("Do not build the solution before packing. Implies --no-restore.")]
  private readonly bool NoBuild = false;

  [Parameter("The API key for the NuGet source.", List = false)]
  [Secret]
  private readonly string NugetTestToken;

  [Parameter("The API key for the NuGet source.")]
  [Secret]
  private string NugetToken;

  [Parameter("The NuGet source.")]
  private readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

  protected override void OnBuildInitialized()
  {
    NugetToken ??= NugetTestToken;
  }

  private Target Clean => _ => _
      .Before(Restore)
      .Executes(() =>
      {
        DotNetTasks.DotNetClean(_ => _
          .SetProject(Solution)
          .SetConfiguration(Configuration));
      });

  private Target Restore => _ => _
      .OnlyWhenStatic(() => !NoRestore)
      .Executes(() =>
      {
        DotNetTasks.DotNetToolRestore();

        DotNetTasks.DotNetRestore(_ => _
          .SetProjectFile(Solution)
          .EnableForce());
      });

  private Target Compile => _ => _
      .DependsOn(Restore)
      .OnlyWhenStatic(() => !NoBuild)
      .Executes(() =>
      {
        DotNetTasks.DotNetBuild(_ => _
          .EnableNoRestore()
          .EnableNoIncremental()
          .SetConfiguration(Configuration)
          .SetContinuousIntegrationBuild(IsServerBuild)
          .SetProjectFile(Solution));
      });

  private Target Test => _ => _
      .DependsOn(Compile)
      .Executes(() =>
      {
        var testProjects = Solution.AllProjects.Where(p => p.GetProperty("IsTestProject")?.EqualsOrdinalIgnoreCase("true") == true);

        foreach (var project in testProjects)
        {
          DotNetTasks.DotNetRun(_ => _
          .EnableNoBuild()
          .SetConfiguration(Configuration)
          .SetProjectFile(project));
        }
      });

  private Target Pack => _ => _
      .DependsOn(Compile)
      .Produces(OutputDirectory / "*.nupgk")
      .Executes(() =>
      {
        DotNetTasks.DotNetPack(_ => _
          .EnableNoBuild()
          .SetConfiguration(Configuration)
          .SetProject(Solution)
          .SetOutputDirectory(OutputDirectory));
      });

  private Target Push => _ => _
      .DependsOn(Pack)
      .After(Test)
      .Requires(() => NuGetSource)
      .Requires(() => NugetToken)
      .Executes(() =>
      {
        DotNetTasks.DotNetNuGetPush(_ => _
          .EnableSkipDuplicate()
          .SetApiKey(NugetTestToken)
          .SetSource(NuGetSource)
          .SetTargetPath(OutputDirectory / "*.nupkg"));
      });

  private Target Info => _ => _
      .Requires(() => GitVersion)
      .Requires(() => GitRepository)
      .Executes(() =>
      {
        Log.Information("Configuration =        {Configuration}", Configuration);
        Log.Information("AssemblyVersion =      {AssemblySemVer}", GitVersion.AssemblySemVer);
        Log.Information("FileVersion =          {AssemblySemFileVer}", GitVersion.AssemblySemFileVer);
        Log.Information("FullSemVer =           {FullSemVer}", GitVersion.FullSemVer);
        Log.Information("InformationalVersion = {InformationalVersion}", GitVersion.InformationalVersion);
        Log.Information("Commit =               {Commit}", GitRepository.Commit);
        Log.Information("Branch =               {Branch}", GitRepository.Branch);
        Log.Information("IsLocalBuild =         {IsLocalBuild}", IsLocalBuild);
      });

  private Target Tag => _ => _
      .Requires(() => GitVersion)
      .Before(Compile)
      .Executes(() =>
      {
        GitTasks.Git(arguments: $"tag {GitVersion.MajorMinorPatch}");
        GitTasks.Git(arguments: $"push origin {GitVersion.MajorMinorPatch}");
      });

  private Target Release => _ => _
      .DependsOn(Tag, Test, Push)
      .Executes(() =>
      {
        GitTasks.Git(arguments: $"tag {GitVersion.SemVer}");
        GitTasks.Git(arguments: $"push origin {GitVersion.SemVer}");
      });
}

#pragma warning restore IDE0051 // Remove unused private members
