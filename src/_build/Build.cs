using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitVersion;
using Serilog;

#pragma warning disable IDE0051 // Remove unused private members

[DotNetVerbosityMapping]
[GitHubActions(
  "dotnet",
  GitHubActionsImage.WindowsLatest,
  //On = [GitHubActionsTrigger.Push],
  OnPushBranches = ["main"],
  OnPullRequestBranches = ["main"],
  InvokedTargets = [nameof(Test), nameof(Push)])]
internal class Build : NukeBuild
{
  [Solution(GenerateProjects = true)]
  private readonly Solution Solution;

  [GitVersion]
  private readonly GitVersion GitVersion;

  [GitRepository]
  private readonly GitRepository GitRepository;

  private readonly AbsolutePath ToolManifest = RootDirectory / "dotnet-tools.json";

  private readonly AbsolutePath OutputDirectory = (RootDirectory / "_output").CreateOrCleanDirectory();

  public static int Main() => Execute<Build>(x => x.Compile);

  [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
  private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  [Parameter("Do not restore the solution before building.")]
  private readonly bool NoRestore = false;

  [Parameter("Do not build the solution before packing. Implies --no-restore.")]
  private readonly bool NoBuild = false;

  [Parameter("The API key for the NuGet source.")]
  [Secret]
  private readonly string NuGetApiKey;

  [Parameter("The NuGet source.")]
  private readonly string NuGetSource = "https://apiint.nugettest.org/v3/index.json";

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
        DotNetTasks.DotNetToolRestore(_ => _
          .SetConfigFile(ToolManifest));

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
                DotNetTasks.DotNetTest(_ => _
                  .EnableNoBuild()
                  .SetConfiguration(Configuration)
                  .SetProjectFile(Solution));
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
      .Requires(() => NuGetSource)
      .Requires(() => NuGetApiKey)
      .Executes(() =>
      {
        DotNetTasks.DotNetNuGetPush(_ => _
          .EnableSkipDuplicate()
          .SetApiKey(NuGetApiKey)
          .SetSource(NuGetSource)
          .SetTargetPath(OutputDirectory / "*.nupkg"));
      });

  private Target Info => _ => _
      .Requires(() => GitVersion)
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
      .Executes(() =>
      {
        GitTasks.Git(arguments: $"tag {GitVersion.SemVer}");
        GitTasks.Git(arguments: $"push origin {GitVersion.SemVer}");
      });
}

#pragma warning restore IDE0051 // Remove unused private members
