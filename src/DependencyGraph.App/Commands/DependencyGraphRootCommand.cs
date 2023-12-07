// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;

namespace DependencyGraph.App.Commands
{
  public class DependencyGraphRootCommand : RootCommand
  {
    public DependencyGraphRootCommand(IEnumerable<Command> commands) : base ("Dependency-graph helps you analyze the dependencies of .NET SDK style projects or nuget packages.")
    {
      foreach (var command in commands)
        AddCommand(command);
    }
  }
}
