// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace DependencyGraph.Core.Graph
{
  public class DependencyGraph : IDependencyGraph
  {
    public DependencyGraph(string description)
    {
      Description = description;
    }

    public HashSet<IDependencyGraphNode> RootNodes { get; } = new HashSet<IDependencyGraphNode>();
    public HashSet<IDependencyGraphNode> Nodes { get; } = new HashSet<IDependencyGraphNode>();
    public string Description { get; }
    IReadOnlyCollection<IDependencyGraphNode> IDependencyGraph.RootNodes => RootNodes;
    public bool IsEmpty => !Nodes.Any();

    public void AddDependency(DependencyGraphNodeBase from, IDependencyGraphNode to)
    {
      Nodes.Add(from);
      from.Dependencies.Add(to);
    }

    public void AddRoot(IDependencyGraphNode node)
    {
      Nodes.Add(node);
      RootNodes.Add(node);
    }

    public DependencyGraph CombineWith(DependencyGraph otherGraph)
    {
      foreach (var rootNode in otherGraph.RootNodes)
        AddRoot(rootNode);

      foreach (var node in otherGraph.Nodes)
        Nodes.Add(node);

      return this;
    }
  }
}
