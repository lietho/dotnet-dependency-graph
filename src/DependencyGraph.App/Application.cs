// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using DependencyGraph.App.Commands;

namespace DependencyGraph.App
{
  internal class Application
  {
    private readonly RootCommand _rootCommand;

    public Application(RootCommand rootCommand)
    {
      _rootCommand = rootCommand;
    }

    public async Task RunAsync(string[] args)
    {
      // Disable the built-in exception handler so we can handle our own exception types explicitly.
      var configuration = new InvocationConfiguration
      {
        EnableDefaultExceptionHandler = false
      };

      var parseResult = _rootCommand.Parse(args);

      try
      {
        await parseResult.InvokeAsync(configuration);
      }
      catch (CommandException ex)
      {
        await Console.Error.WriteLineAsync(ex.Message);
      }
      catch (ApplicationException ex)
      {
        await Console.Error.WriteLineAsync(ex.Message);
      }
    }
  }
}
