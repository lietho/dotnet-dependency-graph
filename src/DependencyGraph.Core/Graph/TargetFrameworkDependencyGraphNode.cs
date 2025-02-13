// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Xml.Linq;

namespace DependencyGraph.Core.Graph
{
  public class TargetFrameworkDependencyGraphNode : DependencyGraphNodeBase, IEquatable<TargetFrameworkDependencyGraphNode>
  {
    public TargetFrameworkDependencyGraphNode(string projectName, string targetFrameworkIdentifier)
    {
      ProjectName = projectName;
      TargetFrameworkIdentifier = targetFrameworkIdentifier;
    }

    public string ProjectName { get; }
    public string TargetFrameworkIdentifier { get; }
    public override string Name => ToString();

    public override string ToString() => $"{ProjectName}/{TargetFrameworkIdentifier}";

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
        return 23 * StringComparer.OrdinalIgnoreCase.GetHashCode(TargetFrameworkIdentifier) ^ 13 * StringComparer.OrdinalIgnoreCase.GetHashCode(ProjectName);
      }
    }

    public bool Equals(TargetFrameworkDependencyGraphNode? other)
      => TargetFrameworkIdentifier.Equals(other?.TargetFrameworkIdentifier, StringComparison.OrdinalIgnoreCase) &&
         ProjectName.Equals(other?.ProjectName, StringComparison.OrdinalIgnoreCase);
  }
}
