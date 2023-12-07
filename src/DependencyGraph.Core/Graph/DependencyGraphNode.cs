﻿// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace DependencyGraph.Core.Graph
{
  public abstract class DependencyGraphNode : DependencyGraphNodeBase, IEquatable<DependencyGraphNode>
  {
    protected DependencyGraphNode(string name)
    {
      Name = name;
    }

    public string Name { get; }

    public override string ToString() => $"{Name}";

    public override bool Equals(object? obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      return Equals((DependencyGraphNode)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return 13 * Name.GetHashCode();
      }
    }

    public bool Equals(DependencyGraphNode? other) => Name.Equals(other?.Name, StringComparison.OrdinalIgnoreCase);
  }
}
