// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using DependencyGraph.Core.Graph;

namespace DependencyGraph.Core.Visualizer
{
  public interface IDependencyGraphVisualizer
  {
    Task VisualizeAsync(IDependencyGraph graph);
  }
}
