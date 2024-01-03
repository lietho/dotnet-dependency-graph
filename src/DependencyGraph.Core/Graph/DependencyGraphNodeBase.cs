// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace DependencyGraph.Core.Graph
{
  public abstract class DependencyGraphNodeBase : IDependencyGraphNode
  {
    IReadOnlyCollection<IDependencyGraphNode> IDependencyGraphNode.Dependencies => Dependencies;
    public HashSet<IDependencyGraphNode> Dependencies { get; } = new HashSet<IDependencyGraphNode>();
  }
}
