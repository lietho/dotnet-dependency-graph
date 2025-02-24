// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Reflection;

namespace DependencyGraph.App
{
  internal static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddCommands(this IServiceCollection services, params Assembly[] assemblies)
    {
      var commandType = typeof(Command);
      var rootCommandType = typeof(RootCommand);

      var commands = assemblies
          .SelectMany(_ => _.GetTypes())
          .Where(commandType.IsAssignableFrom)
          .Where(type => !rootCommandType.IsAssignableFrom(type))
          .Where(type => !type.IsAbstract);

      foreach (var command in commands)
        services.AddSingleton(commandType, command);

      return services;
    }

    public static IServiceCollection AddRootCommand<T>(this IServiceCollection services) where T : RootCommand => services.AddSingleton<RootCommand, T>();
  }
}
