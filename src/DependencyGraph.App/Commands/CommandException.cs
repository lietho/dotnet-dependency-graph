// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;

namespace DependencyGraph.App.Commands
{
  [Serializable]
  public class CommandException : Exception
  {
    public Command Command { get; }

    public CommandException(Command command)
    {
      Command = command;
    }

    public CommandException(string message, Command command) : base(message)
    {
      Command = command;
    }

    public CommandException(string message, Command command, Exception inner) : base(message, inner)
    {
      Command = command;
    }
  }
}
