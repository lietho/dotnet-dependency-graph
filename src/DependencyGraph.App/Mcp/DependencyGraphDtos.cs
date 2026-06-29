// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace DependencyGraph.App.Mcp
{
  /// <summary>
  /// The kind of a dependency graph node, mirroring the concrete node types in DependencyGraph.Core.
  /// </summary>
  public enum DependencyNodeKind
  {
    RootProject,
    TargetFramework,
    Project,
    Package,
    FrameworkProvided,
    Unknown
  }

  /// <summary>
  /// A single node in the dependency graph together with its (already expanded) child dependencies.
  /// </summary>
  public record DependencyNodeDto(
    string Name,
    DependencyNodeKind Kind,
    string? Version,
    string? ProjectName,
    string? TargetFramework,
    IReadOnlyList<DependencyNodeDto> Dependencies);

  /// <summary>
  /// The full dependency graph for a project or solution, rooted at one node per project (and target framework).
  /// </summary>
  public record DependencyGraphDto(
    string Description,
    IReadOnlyList<DependencyNodeDto> RootNodes);

  /// <summary>
  /// The result of tracing the paths from the root nodes to a specified dependency. Each path is a flat,
  /// ordered list of nodes from a root to the matched dependency.
  /// </summary>
  public record TraceResultDto(
    int PathCount,
    IReadOnlyList<IReadOnlyList<DependencyNodeDto>> Paths);

  /// <summary>
  /// A distinct NuGet package (name + resolved version) referenced anywhere in the graph.
  /// </summary>
  public record PackageDto(string Name, string Version);

  /// <summary>
  /// A package that is resolved to more than one version within the graph (a potential version conflict).
  /// </summary>
  public record VersionConflictDto(string Name, IReadOnlyList<string> Versions);
}
