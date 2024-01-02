// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DependencyGraph.Core.Graph.Factory
{
  public class DependencyGraphFactoryOptions
  {
    public string[] Includes { get; set; } = [".*"];

    public string[] Excludes { get; set; } = [];

    public int? MaxDepth { get; set; }
  }
}
