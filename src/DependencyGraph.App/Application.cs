// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using DependencyGraph.App.Commands;

namespace DependencyGraph.App
{
  public class Application
  {
    private readonly RootCommand _rootCommand;

    public Application(RootCommand rootCommand)
    {
      _rootCommand = rootCommand;
    }

    public async Task RunAsync(string[] args)
    {
      var commandLineBuilder = new CommandLineBuilder(_rootCommand);

      commandLineBuilder.AddMiddleware(async (context, next) =>
      {
        try 
        {
          await next(context);
        } 
        catch (CommandException ex) 
        {
          await Console.Error.WriteLineAsync(ex.Message);
        }
      });

      commandLineBuilder.UseDefaults();
      var parser = commandLineBuilder.Build();
      await parser.InvokeAsync(args);
    }
  }
}
