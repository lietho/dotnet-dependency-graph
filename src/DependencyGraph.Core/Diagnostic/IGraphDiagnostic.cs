// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using DependencyGraph.Core.Graph;

namespace DependencyGraph.Core.Diagnostic
{
  public interface IGraphDiagnostic<TResult>
  {
    TResult Perform(IDependencyGraph dependencyGraph);
  }
}
