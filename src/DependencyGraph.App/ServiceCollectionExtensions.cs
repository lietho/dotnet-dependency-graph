// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyGraph.App
{
  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddCommands(this IServiceCollection services, params Assembly[] assemblies)
    {
      var commandType = typeof(Command);
      var rootCommandType = typeof(RootCommand);

      var commands = assemblies
          .SelectMany(_ => _.GetExportedTypes())
          .Where(type => commandType.IsAssignableFrom(type))
          .Where(type => !rootCommandType.IsAssignableFrom(type))
          .Where(type => !type.IsAbstract);

      foreach (var command in commands)
        services.AddSingleton(commandType, command);

      return services;
    }

    public static IServiceCollection AddRootCommand<T>(this IServiceCollection services) where T : RootCommand => services.AddSingleton<RootCommand, T>();
  }
}
