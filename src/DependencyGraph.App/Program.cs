// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using DependencyGraph.App;
using DependencyGraph.App.Commands;
using DependencyGraph.Core.Graph.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NuGet.Common;
using Application = DependencyGraph.App.Application;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
      services.AddTransient<Application>();
      services.AddCommands(typeof(Program).Assembly);
      services.AddRootCommand<DependencyGraphRootCommand>();
      services.AddSingleton(NullLogger.Instance);
      services.AddSingleton<IDependencyGraphFactory, DependencyGraphFactory>();
    })
    .Build();

using (var serviceScope = host.Services.CreateScope())
{
  var services = serviceScope.ServiceProvider;

  try
  {
    var app = services.GetRequiredService<Application>();
    await app.RunAsync(args);
  }
  catch (Exception ex)
  {
    await Console.Error.WriteLineAsync($"An error occured: {ex.Message}");
  }
}
