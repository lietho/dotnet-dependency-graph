﻿// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using DependencyGraph.Core.Graph;

namespace DependencyGraph.Core.Visualizer.Console
{
  public class ConsoleDependencyGraphVisualizer : IDependencyGraphVisualizer
  {
    private class Colors
    {
      public Colors(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
      {
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
      }

      public ConsoleColor ForegroundColor { get; }
      public ConsoleColor BackgroundColor { get; }
    }

    private readonly ConsoleDependencyGraphVisualizerOptions _options;


    public ConsoleDependencyGraphVisualizer(ConsoleDependencyGraphVisualizerOptions options)
    {
      _options = options;
    }

    public async Task VisualizeAsync(IDependencyGraph graph)
    {
      foreach (var rootNode in graph.RootNodes)
      {
        await VisualizeInternalAsync(rootNode, _options);
      }
    }

    private static async Task VisualizeInternalAsync(IDependencyGraphNode node, ConsoleDependencyGraphVisualizerOptions options, int currentIndent = 0)
    {
      await PrintNodeAsync(node, options, currentIndent);

      foreach (var dependency in node.Dependencies)
        await VisualizeInternalAsync(dependency, options, currentIndent + options.IndentSize);
    }

    private static async Task PrintNodeAsync(IDependencyGraphNode node, ConsoleDependencyGraphVisualizerOptions options, int currentIndent)
    {
      var oldColors = new Colors(System.Console.ForegroundColor, System.Console.BackgroundColor);
      var newColors = new Colors(options.GetForegroundColor?.Invoke(node, oldColors.ForegroundColor) ?? oldColors.ForegroundColor, options.GetBackgroundColor?.Invoke(node, oldColors.BackgroundColor) ?? oldColors.BackgroundColor);

      SetColors(newColors);
      await System.Console.Out.WriteLineAsync($"{new string(' ', currentIndent)}{options.Renderer(node) ?? string.Empty}");
      SetColors(oldColors);
    }

    private static void SetColors(Colors colors)
    {
      System.Console.ForegroundColor = colors.ForegroundColor;
      System.Console.BackgroundColor = colors.BackgroundColor;
    }
  }
}
