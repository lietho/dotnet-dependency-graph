// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using NuGet.ProjectModel;
using NuGet.Versioning;

namespace DependencyGraph.Core.Graph.Factory
{
  public class DependencyGraphFactory : IDependencyGraphFactory
  {
    public IDependencyGraph FromLockFile(LockFile lockFile, string[] includes, string[] excludes, int? maxDepth)
    {
      var graph = new DependencyGraph();

      AddProjectToGraph(graph, lockFile, includes, excludes, maxDepth);

      return graph;
    }

    private static void AddProjectToGraph(DependencyGraph graph, LockFile lockFile, string[] includes, string[] excludes, int? maxDepth)
    {
      var rootNode = new ProjectDependencyGraphNode(lockFile.PackageSpec.RestoreMetadata.ProjectName);

      graph.AddRoot(rootNode);

      foreach (var dependencyGroup in lockFile.ProjectFileDependencyGroups)
      {
        var targetFrameworkDependencyGraphNode = CreateNodeForDependencyGroup(dependencyGroup, lockFile.GetTarget(dependencyGroup.FrameworkName, null), lockFile, includes, excludes, maxDepth);
        graph.AddDependency(rootNode, targetFrameworkDependencyGraphNode);
      }
    }

    private static IDependencyGraphNode CreateNodeForDependencyGroup(ProjectFileDependencyGroup dependencyGroup, LockFileTarget lockFileTarget, LockFile lockFile, string[] includes, string[] excludes, int? maxDepth)
    {
      var node = new TargetFrameworkDependencyGraphNode(dependencyGroup.FrameworkName);

      foreach (var dependency in dependencyGroup.Dependencies)
      {
        var libraryName = dependency.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        var versionRange = lockFile.PackageSpec.TargetFrameworks.Single(framework => framework.TargetAlias.Equals(dependencyGroup.FrameworkName, StringComparison.OrdinalIgnoreCase)).Dependencies.FirstOrDefault(dep => dep.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase))?.LibraryRange.VersionRange;

        if (ShouldInclude(libraryName, includes, excludes) == false)
          continue;

        var directDependencyNode = CreateNodeForLibrary(libraryName, versionRange, lockFileTarget, includes, excludes, 1, maxDepth);
        
        node.Dependencies.Add(directDependencyNode);
      }

      return node;
    }

    private static bool ShouldInclude(string libraryName, string[] includes, string[] excludes)
      => includes.Any(_ => Regex.IsMatch(libraryName, _)) && excludes.Any(_ => Regex.IsMatch(libraryName, _)) == false;

    private static IDependencyGraphNode CreateNodeForLibrary(string name, VersionRange? versionRange, LockFileTarget lockFileTarget, string[] includes, string[] excludes, int currentDepth, int? maxDepth)
    {
      var library = lockFileTarget.Libraries
        .Where(lib => name.Equals(lib.Name, StringComparison.OrdinalIgnoreCase))
        .FindBestMatch(versionRange, lib => lib.Version ?? new NuGetVersion(0, 0, 1)) 
        ?? throw new ApplicationException($"Can not find best match for version range '{versionRange}' of library '{name}'.");

      var node = CreateNode(library);

      if (currentDepth >= (maxDepth ?? int.MaxValue))
        return node;

      foreach (var dependency in library.Dependencies)
      {
        if (ShouldInclude(dependency.Id, includes, excludes) == false)
          continue;

        node.Dependencies.Add(CreateNodeForLibrary(dependency.Id, dependency.VersionRange, lockFileTarget, includes, excludes, currentDepth + 1, maxDepth));
      }

      return node;
    }

    private static DependencyGraphNode CreateNode(LockFileTargetLibrary library) => library.Type switch
    {
      "package" => new PackageDependencyGraphNode(library.Name ?? string.Empty, library.Version ?? new NuGetVersion(0, 0, 1)),
      "project" => new ProjectDependencyGraphNode(library.Name ?? string.Empty),
      _ => throw new NotSupportedException($"Library type '{library.Type}' is not supported yet")
    };
  }
}
