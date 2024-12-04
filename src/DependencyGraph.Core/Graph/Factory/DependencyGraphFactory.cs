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
    private static readonly string[] s_separator = new[] { " " };

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

      return FromProjectFileInternal(project, projectFileInfo);
    }

    public IDependencyGraph FromProject(Project project) => FromProjectFileInternal(project, new FileInfo(project.FullPath));

    private IDependencyGraph FromProjectFileInternal(Project project, FileInfo projectFileInfo)
    {
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

        if (dependencyGraph.AllNodes.Except(dependencyGraph.RootNodes).Contains(graph.RootNodes.Single(), new RootDependencyGraphNodeEqualityComparer()))
          return false;
      }

      return true;
    }

    private void AddProjectToGraph(DependencyGraph graph, LockFile lockFile)
    {
      var rootNode = new RootProjectDependencyGraphNode(lockFile.PackageSpec.RestoreMetadata.ProjectName);

      graph.AddRoot(rootNode);

      foreach (var dependencyGroup in lockFile.ProjectFileDependencyGroups)
      {
        var lockFileTarget = GetLockFileTarget(lockFile, NuGetFramework.Parse(dependencyGroup.FrameworkName));
        var targetFrameworkDependencyGraphNode = CreateNodeForDependencyGroup(graph, dependencyGroup, lockFileTarget, lockFile);

        graph.AddDependency(rootNode, targetFrameworkDependencyGraphNode);
      }
    }

    private static LockFileTarget GetLockFileTarget(LockFile lockFile, NuGetFramework nuGetFramework)
    {
      var matchingTargets = lockFile.Targets.Where(lockFileTarget => lockFileTarget.TargetFramework.Equals(nuGetFramework)).ToList();

      if (matchingTargets.Count == 0)
        throw new ApplicationException($"Could not find matching lock file target for framework '{nuGetFramework}'");

      if (matchingTargets.Count == 1)
        return matchingTargets[0];

      return matchingTargets.First(t => string.IsNullOrEmpty(t.RuntimeIdentifier));
    }

    private TargetFrameworkDependencyGraphNode CreateNodeForDependencyGroup(DependencyGraph graph, ProjectFileDependencyGroup dependencyGroup, LockFileTarget lockFileTarget, LockFile lockFile)
    {
      var node = new TargetFrameworkDependencyGraphNode(lockFile.PackageSpec.RestoreMetadata.ProjectName, dependencyGroup.FrameworkName);

      foreach (var dependency in dependencyGroup.Dependencies)
      {
        var libraryName = dependency.Split(s_separator, StringSplitOptions.RemoveEmptyEntries)[0];

        if (ShouldInclude(libraryName, _options) == false)
          continue;

        var directDependencyNode = CreateNodeForLibrary(graph, libraryName, lockFileTarget, _options, 1);

        graph.AddDependency(node, directDependencyNode);
      }

      return node;
    }

    private static bool ShouldInclude(string libraryName, DependencyGraphFactoryOptions options)
      => options.Includes.Any(_ => Regex.IsMatch(libraryName, _)) && options.Excludes.Any(_ => Regex.IsMatch(libraryName, _)) == false;

    private static DependencyGraphNodeBase CreateNodeForLibrary(DependencyGraph graph, string name, LockFileTarget lockFileTarget, DependencyGraphFactoryOptions options, int currentDepth)
    {
      var library = lockFileTarget.GetTargetLibrary(name) ?? throw new ApplicationException($"Target library with name '{name}' could not be resolved.");
      var node = CreateNode(library);

      if (graph.TryGetExistingNode(node, out var existingNode))
        return existingNode!;

      if (currentDepth >= (options.MaxDepth ?? int.MaxValue))
        return node;

      foreach (var dependency in library.Dependencies)
      {
        if (ShouldInclude(dependency.Id, options) == false)
          continue;

        graph.AddDependency(node, CreateNodeForLibrary(graph, dependency.Id, lockFileTarget, options, currentDepth + 1));
      }

      return node;
    }

    private static DependencyGraphNodeBase CreateNode(LockFileTargetLibrary library) => library.Type switch
    {
      "package" => new PackageDependencyGraphNode(library.Name ?? string.Empty, library.Version ?? new NuGetVersion(0, 0, 1), library),
      "project" => new ProjectDependencyGraphNode(library.Name ?? string.Empty, library),
      _ => throw new NotSupportedException($"Library type '{library.Type}' is not supported yet.")
    };

    private class RootDependencyGraphNodeEqualityComparer : IEqualityComparer<IDependencyGraphNode>
    {
      public bool Equals(IDependencyGraphNode? x, IDependencyGraphNode? y)
      {
        if (x is RootProjectDependencyGraphNode xRootNode && y is ProjectDependencyGraphNode yProjectNode)
          return xRootNode.Name.Equals(yProjectNode.Name, StringComparison.OrdinalIgnoreCase);

        if (x is ProjectDependencyGraphNode xProjectNode && y is RootProjectDependencyGraphNode yRootNode)
          return xProjectNode.Name.Equals(yRootNode.Name, StringComparison.OrdinalIgnoreCase);

        if (x == null && y == null)
          return true;

        if (x == null && y != null || x != null && y == null)
          return false;

        if (x != null)
          return x.Equals(y);

        if (y != null)
          return y.Equals(x);

        return false;
      }

      public int GetHashCode(IDependencyGraphNode obj) => obj.GetHashCode();
    }
  }
}
