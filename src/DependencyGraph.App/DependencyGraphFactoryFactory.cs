// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using DependencyGraph.Core.Graph.Factory;

namespace DependencyGraph.App
{
  public class DependencyGraphFactoryFactory
  {
    public IDependencyGraphFactory Create(DependencyGraphFactoryOptions options) => new DependencyGraphFactory(options);
  }
}
