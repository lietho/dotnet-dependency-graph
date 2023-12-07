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

      graph.RootNodes.Add(rootNode);

      foreach (var dependencyGroup in lockFile.ProjectFileDependencyGroups)
      {
        var targetFrameworkDependencyGraphNode = CreateNodeFor(dependencyGroup, lockFile.Targets.First(_ => _.Name == dependencyGroup.FrameworkName), includes, excludes, maxDepth);
        rootNode.Dependencies.Add(targetFrameworkDependencyGraphNode);
      }
    }

    private static IDependencyGraphNode CreateNodeFor(ProjectFileDependencyGroup dependencyGroup, LockFileTarget lockFileTarget, string[] includes, string[] excludes, int? maxDepth)
    {
      var node = new TargetFrameworkDependencyGraphNode(dependencyGroup.FrameworkName);

      foreach (var dependency in dependencyGroup.Dependencies)
      {
        // TODO: find better solution; how is the string "{project|nuget} >= {version}" created
        var libraryName = dependency.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

        if (ShouldInclude(libraryName, includes, excludes) == false)
          continue;

        var directDependencyNode = CreateNodeForLibrary(libraryName, lockFileTarget, includes, excludes, 1, maxDepth);
        node.Dependencies.Add(directDependencyNode);
      }

      return node;
    }

    private static bool ShouldInclude(string libraryName, string[] includes, string[] excludes)
      => includes.Any(_ => Regex.IsMatch(libraryName, _)) && excludes.Any(_ => Regex.IsMatch(libraryName, _)) == false;

    private static IDependencyGraphNode CreateNodeForLibrary(string libraryName, LockFileTarget lockFileTarget, string[] includes, string[] excludes, int currentDepth, int? maxDepth)
    {
      var library = lockFileTarget.Libraries.First(lib => lib.Name == libraryName);
      var node = CreateNode(library);

      if (currentDepth >= (maxDepth ?? int.MaxValue))
        return node;

      foreach (var dependency in library.Dependencies)
      {
        if (ShouldInclude(dependency.Id, includes, excludes) == false)
          continue;

        node.Dependencies.Add(CreateNodeForLibrary(dependency.Id, lockFileTarget, includes, excludes, currentDepth + 1, maxDepth));
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
