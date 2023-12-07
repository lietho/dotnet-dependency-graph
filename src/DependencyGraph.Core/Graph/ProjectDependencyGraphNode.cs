// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using NuGet.Versioning;

namespace DependencyGraph.Core.Graph
{
  public class ProjectDependencyGraphNode : DependencyGraphNode, IEquatable<ProjectDependencyGraphNode>
  {
    public ProjectDependencyGraphNode(string name) : base(name)
    {
    }

    public bool Equals(ProjectDependencyGraphNode? other) => base.Equals(other);
  }
}
