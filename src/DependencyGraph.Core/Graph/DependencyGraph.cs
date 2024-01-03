// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace DependencyGraph.Core.Graph
{
  public class DependencyGraph : IDependencyGraph
  {
    public DependencyGraph()
    {
    }

    public HashSet<IDependencyGraphNode> RootNodes { get; } = new HashSet<IDependencyGraphNode>();

    public HashSet<IDependencyGraphNode> Nodes { get; } = new HashSet<IDependencyGraphNode>();

    IReadOnlyCollection<IDependencyGraphNode> IDependencyGraph.RootNodes => RootNodes;

    public void AddDependency(DependencyGraphNode from, IDependencyGraphNode to)
    {
      Nodes.Add(from);
      Nodes.Add(to);
      from.Dependencies.Add(to);
    }

    public void AddRoot(IDependencyGraphNode node)
    {
      Nodes.Add(node);
      RootNodes.Add(node);
    }
  }
}
