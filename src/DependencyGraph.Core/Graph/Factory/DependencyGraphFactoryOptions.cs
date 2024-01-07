// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace DependencyGraph.Core.Graph.Factory
{
  public class DependencyGraphFactoryOptions
  {
    public string[] Includes { get; set; } = new[] { ".*" };

    public string[] Excludes { get; set; } = Array.Empty<string>();

    public int? MaxDepth { get; set; }
  }
}
