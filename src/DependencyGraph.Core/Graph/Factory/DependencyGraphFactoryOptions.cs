// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using DependencyGraph.Core.Graph;

namespace DependencyGraph.Core.Graph.Factory
{
  public class DependencyGraphFactoryOptions
  {
    public string[] Includes { get; set; } = new[] { ".*" };

    public string[] Excludes { get; set; } = Array.Empty<string>();

    public int? MaxDepth { get; set; }

    /// <summary>
    /// When <c>true</c>, dependencies that are not present in the lock file target because they are provided
    /// by the shared framework (.NET package pruning) are omitted from the graph instead of being represented
    /// as <see cref="FrameworkProvidedDependencyGraphNode"/>.
    /// </summary>
    public bool ExcludeFrameworkProvidedDependencies { get; set; }
  }
}
