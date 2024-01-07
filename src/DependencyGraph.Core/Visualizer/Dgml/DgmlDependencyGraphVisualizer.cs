// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DependencyGraph.Core.Graph;
using DependencyGraph.Core.Visualizer.Dgml.Models;

namespace DependencyGraph.Core.Visualizer.Dgml
{
  public class DgmlDependencyGraphVisualizer : IDependencyGraphVisualizer
  {
    private readonly DgmlDependencyGraphVisualizerOptions _dgmlDependencyGraphVisualizerOptions;

    private const string TargetFrameworkDependencyCategoryId = "FrameworkDependency";
    private const string ProjectDependencyCategoryId = "ProjectDependency";
    private const string PackageDependencyCategoryId = "PackageDependency";

    public DgmlDependencyGraphVisualizer(DgmlDependencyGraphVisualizerOptions dgmlDependencyGraphVisualizerOptions)
    {
      _dgmlDependencyGraphVisualizerOptions = dgmlDependencyGraphVisualizerOptions;
    }

    public Task VisualizeAsync(IDependencyGraph graph)
    {
      var directedGraph = CreateDirectedGraph();

      CreateCategories(directedGraph);
      CreateNodesAndLinks(graph, directedGraph);
      SerializeGraph(directedGraph, _dgmlDependencyGraphVisualizerOptions.OutputFilePath);

      return Task.CompletedTask;
    }

    private static DirectedGraph CreateDirectedGraph() => new DirectedGraph()
    {
      Title = $"Dependencies of TODO", // TODO: add a name property or something to IDependencyGraph
      GraphDirection = GraphDirectionEnum.TopToBottom,
      GraphDirectionSpecified = true,
      Layout = LayoutEnum.DependencyMatrix
    };

    private void CreateCategories(DirectedGraph graph) =>
      graph.Categories = _dgmlDependencyGraphVisualizerOptions.Categories.ToArray();

    private void CreateNodesAndLinks(IDependencyGraph graph, DirectedGraph directedGraph)
    {
      var nodeMap = new Dictionary<IDependencyGraphNode, DirectedGraphNode>();
      var links = new List<DirectedGraphLink>();

      foreach (var rootNode in graph.RootNodes)
        links.AddRange(CreateLinks(rootNode, nodeMap));

      directedGraph.Links = links.ToArray();
      directedGraph.Nodes = nodeMap.Values.ToArray();
    }

    private List<DirectedGraphLink> CreateLinks(IDependencyGraphNode node, Dictionary<IDependencyGraphNode, DirectedGraphNode> nodeMap)
    {
      var links = new List<DirectedGraphLink>();

      foreach (var dependency in node.Dependencies)
      {
        var link = new DirectedGraphLink
        {
          Source = GetOrCreateNode(node, nodeMap).Id,
          Target = GetOrCreateNode(dependency, nodeMap).Id
        };

        links.Add(link);
        links.AddRange(CreateLinks(dependency, nodeMap));
      }

      return links;
    }

    private DirectedGraphNode GetOrCreateNode(IDependencyGraphNode node, Dictionary<IDependencyGraphNode, DirectedGraphNode> nodeMap)
    {
      if (nodeMap.TryGetValue(node, out var graphNode))
        return graphNode;

      graphNode = new DirectedGraphNode
      {
        Id = node.ToString(),
        Label = node.ToString(),
        Category1 = _dgmlDependencyGraphVisualizerOptions.AssignCategory(node)
      };

      nodeMap.Add(node, graphNode);
      return graphNode;
    }

    private static void SerializeGraph(DirectedGraph graph, string outputFilePath)
    {
      var serializer = new XmlSerializer(typeof(DirectedGraph));
      using var streamWriter = new StreamWriter(outputFilePath);
      serializer.Serialize(streamWriter, graph);
    }
  }
}
