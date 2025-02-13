// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using DependencyGraph.Core.Graph;

namespace DependencyGraph.Core.Diagnostic
{
  public abstract class GraphDiagnosticBase<TResult> : IGraphDiagnostic<TResult>
  {
    public abstract TResult Perform(IDependencyGraph dependencyGraph);

    protected IEnumerable<IImmutableList<IDependencyGraphNode>> TraverseDepthFirst(IDependencyGraph dependencyGraph)
    {
      foreach (var rootNode in dependencyGraph.RootNodes)
      {
        var path = ImmutableList.Create(rootNode);

        yield return path;

        foreach (var p in TraverseDepthFirst(rootNode, path))
          yield return p;
      }
    }

    protected IEnumerable<IImmutableList<IDependencyGraphNode>> TraverseDepthFirst(IDependencyGraphNode node, IImmutableList<IDependencyGraphNode> path)
    {
      foreach (var child in node.Dependencies)
      {
        var newPath = path.Add(child);

        yield return newPath;

        foreach (var p in TraverseDepthFirst(child, newPath))
          yield return p;
      }
    }
  }
}
