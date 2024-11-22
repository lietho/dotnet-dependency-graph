// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace DependencyGraph.Core.Graph
{
  public class DependencyGraph : IDependencyGraph
  {
    private readonly Dictionary<DependencyGraphNodeBase, DependencyGraphNodeBase> _nodes = new Dictionary<DependencyGraphNodeBase, DependencyGraphNodeBase>();

    public DependencyGraph(string description)
    {
      Description = description;
    }

    public HashSet<DependencyGraphNodeBase> RootNodes { get; } = new HashSet<DependencyGraphNodeBase>();
    public string Description { get; }
    IReadOnlyCollection<IDependencyGraphNode> IDependencyGraph.RootNodes => RootNodes;
    public bool IsEmpty => _nodes.Count == 0;
    public IReadOnlyCollection<IDependencyGraphNode> AllNodes => _nodes.Values;

    public void AddDependency(DependencyGraphNodeBase from, DependencyGraphNodeBase to)
    {
      _nodes[from] = from;
      _nodes[to] = to;
      from.Dependencies.Add(to);
    }

    public void AddRoot(DependencyGraphNodeBase node)
    {
      _nodes.Add(node, node);
      RootNodes.Add(node);
    }

    public DependencyGraph CombineWith(DependencyGraph otherGraph)
    {
      foreach (var rootNode in otherGraph.RootNodes)
        AddRoot(rootNode);

      foreach (var node in otherGraph._nodes.Values)
        _nodes[node] = node;

      return this;
    }

    public bool TryGetExistingNode(DependencyGraphNodeBase node, out DependencyGraphNodeBase? dependencyGraphNode) => _nodes.TryGetValue(node, out dependencyGraphNode);
  }
}
