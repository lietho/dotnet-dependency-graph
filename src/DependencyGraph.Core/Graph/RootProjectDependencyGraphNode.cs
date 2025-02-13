// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace DependencyGraph.Core.Graph
{
  public class RootProjectDependencyGraphNode : DependencyGraphNodeBase, IEquatable<RootProjectDependencyGraphNode>
  {
    public RootProjectDependencyGraphNode(string name)
    {
      Name = name;
    }

    public override string Name { get; }

    public override bool Equals(object? obj)
    {
      if (obj == null || GetType() != obj.GetType())
      {
        return false;
      }

      return Equals((RootProjectDependencyGraphNode)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return 73 * Name.GetHashCode();
      }
    }

    public bool Equals(RootProjectDependencyGraphNode? other) => Name.Equals(other?.Name, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Name;
  }
}
