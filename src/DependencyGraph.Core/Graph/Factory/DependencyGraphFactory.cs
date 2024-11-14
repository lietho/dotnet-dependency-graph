// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NuGet.Frameworks;
using NuGet.ProjectModel;
using NuGet.Versioning;

namespace DependencyGraph.Core.Graph.Factory
{
  public class DependencyGraphFactory : IDependencyGraphFactory
  {
    private readonly DependencyGraphFactoryOptions _options;

    public DependencyGraphFactory(DependencyGraphFactoryOptions? options = default)
    {
      _options = options ?? new DependencyGraphFactoryOptions();
    }

    public IDependencyGraph FromLockFile(LockFile lockFile)
    {
      var graph = new DependencyGraph(lockFile.PackageSpec.RestoreMetadata.ProjectName);

      AddProjectToGraph(graph, lockFile);

      return graph;
    }

    public IDependencyGraph FromProjectFile(FileInfo projectFileInfo)
    {
      var project = new Project(projectFileInfo.FullName);

      try
      {
        if (project.GetPropertyValue("UsingMicrosoftNETSdk") != "true")
        {
          Console.WriteLine($"Skipping project '{projectFileInfo.Name}': no SDK-style project");
          return new DependencyGraph(Path.GetFileNameWithoutExtension(projectFileInfo.Name));
        }
      }
      finally
      {
        ProjectCollection.GlobalProjectCollection.UnloadProject(project);
      }

      var lockFileInfo = GetLockFileInfo(projectFileInfo.Directory?.EnumerateDirectories("obj").FirstOrDefault(), LockFileFormat.AssetsFileName) 
        ?? throw new ApplicationException($"Could not find assets file {LockFileFormat.AssetsFileName}. Please build the project first.");

      var lockFileFormat = new LockFileFormat();
      var lockFile = lockFileFormat.Read(lockFileInfo.FullName);
      return FromLockFile(lockFile);
    }

    private static FileInfo? GetLockFileInfo(DirectoryInfo? directory, string assetsFileName) => directory?.GetFiles(assetsFileName)?.FirstOrDefault();

    public IDependencyGraph FromSolutionFile(FileInfo solutionFileInfo)
    {
      var dependencyGraphs = new List<DependencyGraph>();
      var solution = SolutionFile.Parse(solutionFileInfo.FullName);

      foreach (var projectInSolution in solution.ProjectsInOrder)
      {
        if (ShouldInclude(projectInSolution.ProjectName, _options) == false)
          continue;

        if (projectInSolution.ProjectType != SolutionProjectType.KnownToBeMSBuildFormat)
        {
          Console.WriteLine($"Skipping project '{projectInSolution.ProjectName}': no MSBuild project");
          continue;
        }

        dependencyGraphs.Add((DependencyGraph)FromProjectFile(new FileInfo(projectInSolution.AbsolutePath)));
      }

      return dependencyGraphs
        .Where(g => IsRootDependency(g, dependencyGraphs))
        .Aggregate(new DependencyGraph(Path.GetFileNameWithoutExtension(solutionFileInfo.Name)), (g1, g2) => g1.CombineWith(g2));
    }

    private static bool IsRootDependency(DependencyGraph graph, List<DependencyGraph> dependencyGraphs)
    {
      foreach (var dependencyGraph in dependencyGraphs)
      {
        if (graph.IsEmpty)
          continue;

        if (dependencyGraph.Nodes.Except(dependencyGraph.RootNodes).Contains(graph.RootNodes.Single()))
          return false;
      }

      return true;
    }

    private void AddProjectToGraph(DependencyGraph graph, LockFile lockFile)
    {
      var rootNode = new ProjectDependencyGraphNode(lockFile.PackageSpec.RestoreMetadata.ProjectName, null);

      graph.AddRoot(rootNode);

      foreach (var dependencyGroup in lockFile.ProjectFileDependencyGroups)
      {
        var targetFrameworkDependencyGraphNode = CreateNodeForDependencyGroup(graph, dependencyGroup, lockFile.Targets.Single(lockFileTarget => lockFileTarget.TargetFramework.Equals(NuGetFramework.Parse(dependencyGroup.FrameworkName))), lockFile);
        graph.AddDependency(rootNode, targetFrameworkDependencyGraphNode);
      }
    }

    private IDependencyGraphNode CreateNodeForDependencyGroup(DependencyGraph graph, ProjectFileDependencyGroup dependencyGroup, LockFileTarget lockFileTarget, LockFile lockFile)
    {
      var node = new TargetFrameworkDependencyGraphNode(lockFile.PackageSpec.RestoreMetadata.ProjectName, dependencyGroup.FrameworkName);

      foreach (var dependency in dependencyGroup.Dependencies)
      {
        var libraryName = dependency.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0];
        var versionRange = lockFile.PackageSpec.TargetFrameworks
          .Single(framework => framework.FrameworkName.Equals(NuGetFramework.Parse(dependencyGroup.FrameworkName)))
          .Dependencies.FirstOrDefault(dep => dep.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase))?.LibraryRange.VersionRange;

        if (ShouldInclude(libraryName, _options) == false)
          continue;

        var directDependencyNode = CreateNodeForLibrary(graph, libraryName, versionRange, lockFileTarget, _options, 1);

        graph.AddDependency(node, directDependencyNode);
      }

      return node;
    }

    private static bool ShouldInclude(string libraryName, DependencyGraphFactoryOptions options)
      => options.Includes.Any(_ => Regex.IsMatch(libraryName, _)) && options.Excludes.Any(_ => Regex.IsMatch(libraryName, _)) == false;

    private static IDependencyGraphNode CreateNodeForLibrary(DependencyGraph graph, string name, VersionRange? versionRange, LockFileTarget lockFileTarget, DependencyGraphFactoryOptions options, int currentDepth)
    {
      var library = lockFileTarget.Libraries
        .Where(lib => name.Equals(lib.Name, StringComparison.OrdinalIgnoreCase))
        .FindBestMatch(versionRange, lib => lib.Version ?? new NuGetVersion(0, 0, 1))
        ?? throw new ApplicationException($"Can not find best match for version range '{versionRange}' of library '{name}'.");

      var node = CreateNode(library);

      if (currentDepth >= (options.MaxDepth ?? int.MaxValue))
        return node;

      foreach (var dependency in library.Dependencies)
      {
        if (ShouldInclude(dependency.Id, options) == false)
          continue;

        graph.AddDependency(node, CreateNodeForLibrary(graph, dependency.Id, dependency.VersionRange, lockFileTarget, options, currentDepth + 1));
      }

      return node;
    }

    private static DependencyGraphNode CreateNode(LockFileTargetLibrary library) => library.Type switch
    {
      "package" => new PackageDependencyGraphNode(library.Name ?? string.Empty, library.Version ?? new NuGetVersion(0, 0, 1), library),
      "project" => new ProjectDependencyGraphNode(library.Name ?? string.Empty, library),
      _ => throw new NotSupportedException($"Library type '{library.Type}' is not supported yet")
    };
  }
}
