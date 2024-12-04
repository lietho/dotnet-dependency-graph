// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NuGet.ProjectModel;

namespace DependencyGraph.Core.Graph.Factory
{
  public interface IDependencyGraphFactory
  {
    IDependencyGraph FromLockFile(LockFile lockFile);

    IDependencyGraph FromSolutionFile(FileInfo solutionFileInfo);

    IDependencyGraph FromProjectFile(FileInfo projectFileInfo);

    IDependencyGraph FromProject(Project project);
  }
}
