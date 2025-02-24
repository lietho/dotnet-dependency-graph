// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using DependencyGraph.Core.Visualizer;
using DependencyGraph.Core.Visualizer.Console;
using DependencyGraph.Core.Visualizer.Dgml;

namespace DependencyGraph.App
{
  internal class DependencyGraphVisualizerFactory
  {
    public IDependencyGraphVisualizer Create(VisualizerType visualizerType, FileInfo? outputFile) => visualizerType switch
    {
      VisualizerType.Console => new ConsoleDependencyGraphVisualizer(new ConsoleDependencyGraphVisualizerOptions()),
      VisualizerType.Dgml => new DgmlDependencyGraphVisualizer(new DgmlDependencyGraphVisualizerOptions(outputFile!.FullName)),
      _ => throw new ArgumentOutOfRangeException(nameof(visualizerType)),
    };
  }
}
