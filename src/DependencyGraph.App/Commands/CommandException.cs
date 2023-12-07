// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DependencyGraph.App.Commands
{

  [Serializable]
  public class CommandException : Exception
  {
    public CommandException() { }
    public CommandException(string message) : base(message) { }
    public CommandException(string message, Exception inner) : base(message, inner) { }
  }
}
