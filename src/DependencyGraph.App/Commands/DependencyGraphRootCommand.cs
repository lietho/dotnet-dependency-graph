// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using DependencyGraph.Core.Graph;
using DependencyGraph.Core.Graph.Factory;
using Microsoft.Build.Construction;
using Microsoft.Build.Locator;
using NuGet.Common;

namespace DependencyGraph.App.Commands
{
  internal class DependencyGraphRootCommand : RootCommand
  {
    public DependencyGraphRootCommand(IEnumerable<Command> commands) : base("Dependency-graph helps you analyze the dependencies of .NET SDK-style projects.")
    {
      // add child commands
      foreach (var command in commands)
        AddCommand(command);
    }
  }
}
