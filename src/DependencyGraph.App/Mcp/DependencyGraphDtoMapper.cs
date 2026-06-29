// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DependencyGraph.Core.Graph;

namespace DependencyGraph.App.Mcp
{
  /// <summary>
  /// Maps the core <see cref="IDependencyGraph"/> model onto the serializable DTOs exposed by the MCP tools.
  /// </summary>
  internal static class DependencyGraphDtoMapper
  {
    public static DependencyGraphDto ToDto(IDependencyGraph graph)
    {
      var rootNodes = graph.RootNodes
        .Select(node => ToDto(node, new HashSet<IDependencyGraphNode>()))
        .ToList();

      return new DependencyGraphDto(graph.Description, rootNodes);
    }

    public static TraceResultDto ToTraceResult(IReadOnlyList<IImmutableList<IDependencyGraphNode>> paths)
    {
      var mappedPaths = paths
        .Select(path => (IReadOnlyList<DependencyNodeDto>)path.Select(ToFlatDto).ToList())
        .ToList();

      return new TraceResultDto(mappedPaths.Count, mappedPaths);
    }

    /// <summary>
    /// Maps a node and its transitive dependencies. <paramref name="ancestors"/> tracks the nodes on the
    /// current path so that cycles (graphs may revisit nodes) terminate instead of recursing forever.
    /// </summary>
    private static DependencyNodeDto ToDto(IDependencyGraphNode node, HashSet<IDependencyGraphNode> ancestors)
    {
      if (!ancestors.Add(node))
        return ToFlatDto(node);

      try
      {
        var dependencies = node.Dependencies
          .Select(dependency => ToDto(dependency, ancestors))
          .ToList();

        return CreateDto(node, dependencies);
      }
      finally
      {
        ancestors.Remove(node);
      }
    }

    private static DependencyNodeDto ToFlatDto(IDependencyGraphNode node) =>
      CreateDto(node, []);

    private static DependencyNodeDto CreateDto(IDependencyGraphNode node, IReadOnlyList<DependencyNodeDto> dependencies) =>
      node switch
      {
        RootProjectDependencyGraphNode root =>
          new DependencyNodeDto(root.Name, DependencyNodeKind.RootProject, null, root.Name, null, dependencies),
        TargetFrameworkDependencyGraphNode tfm =>
          new DependencyNodeDto(tfm.Name, DependencyNodeKind.TargetFramework, null, tfm.ProjectName, tfm.TargetFrameworkIdentifier, dependencies),
        PackageDependencyGraphNode package =>
          new DependencyNodeDto(package.Name, DependencyNodeKind.Package, package.Version.ToString(), null, null, dependencies),
        ProjectDependencyGraphNode project =>
          new DependencyNodeDto(project.Name, DependencyNodeKind.Project, null, project.Name, null, dependencies),
        FrameworkProvidedDependencyGraphNode frameworkProvided =>
          new DependencyNodeDto(frameworkProvided.Name, DependencyNodeKind.FrameworkProvided, null, null, null, dependencies),
        _ => new DependencyNodeDto(node.Name, DependencyNodeKind.Unknown, null, null, null, dependencies)
      };
  }
}
