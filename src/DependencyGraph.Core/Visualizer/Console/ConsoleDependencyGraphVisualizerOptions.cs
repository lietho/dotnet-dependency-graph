// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using DependencyGraph.Core.Graph;

namespace DependencyGraph.Core.Visualizer.Console
{
  public class ConsoleDependencyGraphVisualizerOptions
  {
    public int IndentSize { get; set; } = 3;
    public Func<IDependencyGraphNode, string?> Renderer { get; set; } = item => item.ToString();
    public Func<IDependencyGraphNode, ConsoleColor, ConsoleColor> GetForegroundColor { get; set; } = (node, currentColor) => currentColor;
    public Func<IDependencyGraphNode, ConsoleColor, ConsoleColor> GetBackgroundColor { get; set; } = (node, currentColor) => currentColor;
  }
}
