// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace DependencyGraph.Core.Graph
{
  public interface IDependencyGraph
  {
    string Description { get; }

    IReadOnlyCollection<IDependencyGraphNode> RootNodes { get; }
  }
}
