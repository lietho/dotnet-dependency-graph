// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using NuGet.ProjectModel;

namespace DependencyGraph.Core.Graph
{
  public class ProjectDependencyGraphNode : DependencyGraphNode, IEquatable<ProjectDependencyGraphNode>
  {
    public ProjectDependencyGraphNode(string name, LockFileTargetLibrary targetLibrary) : base(name, targetLibrary)
    {
    }

    public bool Equals(ProjectDependencyGraphNode? other) => base.Equals(other);

    public override bool Equals(object? obj) => base.Equals(obj);

    public override int GetHashCode() => base.GetHashCode();
  }
}
