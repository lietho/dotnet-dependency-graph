// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Build.Construction;
using NuGet.ProjectModel;

namespace DependencyGraph.Core.Graph.Factory
{
  public interface IDependencyGraphFactory
  {
    IDependencyGraph FromLockFile(LockFile lockFile);

    IDependencyGraph FromSolutionFile(SolutionFile solutionFile);
  }
}
