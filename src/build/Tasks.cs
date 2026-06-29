using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.NuGet.Push;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Common.Tools.DotNet.Restore;
using Cake.Common.Tools.DotNet.Run;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    context.DotNetClean(context.SolutionPath.FullPath, new DotNetCleanSettings
    {
      Configuration = context.Configuration,
    });

    context.CleanDirectory(context.OutputDirectory);
  }
}

[TaskName("Restore")]
public sealed class RestoreTask : FrostingTask<BuildContext>
{
  public override bool ShouldRun(BuildContext context) => !context.NoRestore;

  public override void Run(BuildContext context)
  {
    context.RestoreLocalTools();

    context.DotNetRestore(context.SolutionPath.FullPath, new DotNetRestoreSettings
    {
      Force = true,
    });
  }
}

[TaskName("Compile")]
[IsDependentOn(typeof(RestoreTask))]
public sealed class CompileTask : FrostingTask<BuildContext>
{
  public override bool ShouldRun(BuildContext context) => !context.NoBuild;

  public override void Run(BuildContext context)
  {
    context.DotNetBuild(context.SolutionPath.FullPath, new DotNetBuildSettings
    {
      Configuration = context.Configuration,
      NoRestore = true,
      NoIncremental = true,
      MSBuildSettings = new DotNetMSBuildSettings()
          .WithProperty("ContinuousIntegrationBuild", context.IsServerBuild ? "true" : "false"),
    });
  }
}

[TaskName("Test")]
[IsDependentOn(typeof(CompileTask))]
public sealed class TestTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    foreach (var project in context.GetTestProjects())
    {
      context.DotNetRun(project.FullPath, new DotNetRunSettings
      {
        Configuration = context.Configuration,
        NoBuild = true,
      });
    }
  }
}

[TaskName("Pack")]
[IsDependentOn(typeof(CompileTask))]
public sealed class PackTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    context.DotNetPack(context.SolutionPath.FullPath, new DotNetPackSettings
    {
      Configuration = context.Configuration,
      NoBuild = true,
      OutputDirectory = context.OutputDirectory,
    });
  }
}

[TaskName("Push")]
[IsDependentOn(typeof(PackTask))]
public sealed class PushTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    if (string.IsNullOrWhiteSpace(context.NuGetSource))
    {
      throw new CakeException("No NuGet source supplied. Pass --nuget-source or set the NuGetSource environment variable.");
    }

    if (string.IsNullOrWhiteSpace(context.NugetToken))
    {
      throw new CakeException("No NuGet API key supplied. Pass --nuget-token or set the NugetToken environment variable.");
    }

    var packages = context.OutputDirectory.CombineWithFilePath("*.nupkg");

    context.DotNetNuGetPush(packages.FullPath, new DotNetNuGetPushSettings
    {
      ApiKey = context.NugetToken,
      Source = context.NuGetSource,
      SkipDuplicate = true,
    });
  }
}

[TaskName("Info")]
public sealed class InfoTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    var version = context.GitVersion;

    context.Information("Configuration =        {0}", context.Configuration);
    context.Information("AssemblyVersion =      {0}", version.AssemblySemVer);
    context.Information("FileVersion =          {0}", version.AssemblySemFileVer);
    context.Information("FullSemVer =           {0}", version.FullSemVer);
    context.Information("InformationalVersion = {0}", version.InformationalVersion);
    context.Information("Commit =               {0}", version.Sha);
    context.Information("Branch =               {0}", version.BranchName);
    context.Information("IsLocalBuild =         {0}", context.IsLocalBuild);
  }
}

[TaskName("Tag")]
public sealed class TagTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    var version = context.GitVersion.MajorMinorPatch;

    Git(context, $"tag {version}");
    Git(context, $"push origin {version}");
  }

  internal static void Git(BuildContext context, string arguments)
  {
    var exitCode = context.StartProcess("git", new ProcessSettings { Arguments = arguments });
    if (exitCode != 0)
    {
      throw new CakeException($"git {arguments} failed with exit code {exitCode}.");
    }
  }
}

[TaskName("Release")]
[IsDependentOn(typeof(TagTask))]
[IsDependentOn(typeof(TestTask))]
[IsDependentOn(typeof(PushTask))]
public sealed class ReleaseTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    var version = context.GitVersion.SemVer;

    TagTask.Git(context, $"tag {version}");
    TagTask.Git(context, $"push origin {version}");
  }
}

[TaskName("Default")]
[IsDependentOn(typeof(CompileTask))]
public sealed class DefaultTask : FrostingTask<BuildContext>
{
}
