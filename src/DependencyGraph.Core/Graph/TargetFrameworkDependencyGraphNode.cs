// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace DependencyGraph.Core.Graph
{
  public class TargetFrameworkDependencyGraphNode : DependencyGraphNodeBase, IEquatable<TargetFrameworkDependencyGraphNode>
  { 
    public TargetFrameworkDependencyGraphNode(string targetFrameworkIdentifier)
    {
      TargetFrameworkIdentifier = targetFrameworkIdentifier;
    }

    public string TargetFrameworkIdentifier { get; }

    public override string ToString() => $"{TargetFrameworkIdentifier}";

    public override bool Equals(object? obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      return Equals((TargetFrameworkDependencyGraphNode)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return 23 * TargetFrameworkIdentifier.GetHashCode();
      }
    }

    public bool Equals(TargetFrameworkDependencyGraphNode? other) => TargetFrameworkIdentifier.Equals(other?.TargetFrameworkIdentifier, StringComparison.OrdinalIgnoreCase);
  }
}
