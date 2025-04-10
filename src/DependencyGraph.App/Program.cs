﻿// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using DependencyGraph.App.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NuGet.Common;

namespace DependencyGraph.App;

public class Program
{
  public static async Task Main(string[] args)
  {
    using var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) =>
        {
          services.AddTransient<Application>();
          services.AddCommands(typeof(Program).Assembly);
          services.AddRootCommand<DependencyGraphRootCommand>();
          services.AddSingleton(NullLogger.Instance);
          services.AddSingleton<DependencyGraphFactoryFactory>();
          services.AddSingleton<DependencyGraphVisualizerFactory>();
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
        await Console.Error.WriteLineAsync($"An error occurred: {ex.Message}");
      }
    }
  }
}
