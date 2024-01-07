// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using NuGet.ProjectModel;
using NuGet.Versioning;

namespace DependencyGraph.Core.Graph.Factory
{
  public interface IDependencyGraphFactory
  {
    IDependencyGraph FromLockFile(LockFile lockFile);

    IDependencyGraph FromSolutionFile(FileInfo solutionFileInfo);

    IDependencyGraph FromProjectFile(FileInfo projectFileInfo);

    IDependencyGraph FromPackage(string name, NuGetVersion? version = null, Uri? source = null);
  }
}
