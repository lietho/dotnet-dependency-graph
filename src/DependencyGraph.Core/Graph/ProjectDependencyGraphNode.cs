// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using NuGet.ProjectModel;

namespace DependencyGraph.Core.Graph
{
  public class ProjectDependencyGraphNode : DependencyGraphNode, IEquatable<ProjectDependencyGraphNode>
  {
    public ProjectDependencyGraphNode(string name, LockFileTargetLibrary? targetLibrary) : base(name)
    {
      TargetLibrary = targetLibrary;
    }
    public LockFileTargetLibrary? TargetLibrary { get; }

    public bool Equals(ProjectDependencyGraphNode? other) => base.Equals(other);
  }
}
