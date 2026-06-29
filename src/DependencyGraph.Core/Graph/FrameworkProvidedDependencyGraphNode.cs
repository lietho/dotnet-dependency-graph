// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace DependencyGraph.Core.Graph
{
  /// <summary>
  /// Represents a dependency that is declared by another package but is not present as a library in the
  /// lock file target because it is provided by the shared framework (.NET package pruning). Such a node
  /// is always a leaf as there is no resolved library metadata to expand its own dependencies.
  /// </summary>
  public class FrameworkProvidedDependencyGraphNode : DependencyGraphNodeBase, IEquatable<FrameworkProvidedDependencyGraphNode>
  {
    public FrameworkProvidedDependencyGraphNode(string name)
    {
      Name = name;
    }

    public override string Name { get; }

    public override string ToString() => $"{Name} (framework-provided)";

    public override bool Equals(object? obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      return Equals((FrameworkProvidedDependencyGraphNode)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return 13 * StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
      }
    }

    public bool Equals(FrameworkProvidedDependencyGraphNode? other) => Name.Equals(other?.Name, StringComparison.OrdinalIgnoreCase);
  }
}
