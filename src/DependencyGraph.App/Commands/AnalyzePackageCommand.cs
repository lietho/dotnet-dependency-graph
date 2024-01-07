// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using System.CommandLine.Parsing;
using NuGet.Versioning;

namespace DependencyGraph.App.Commands
{
  public class AnalyzePackageCommand : Command
  {
    private readonly Argument<string> _packageNameArgument;
    private readonly Option<NuGetVersion?> _packageVersionOption;
    private readonly Option<Uri?> _packageSourceOption;
    private readonly Option<VisualizerType> _visualizerOption;
    private readonly Option<FileInfo?> _outputFileOption;
    private readonly Option<string[]?> _includeOption;
    private readonly Option<string[]?> _excludeOption;
    private readonly Option<int?> _maxDepthOption;

    public AnalyzePackageCommand() : base("package", "Analyzes the dependencies of a NuGet package from a package repository.")
    {
      _packageNameArgument = new Argument<string>("package name", "The name of the package you want to analyze.");
      AddArgument(_packageNameArgument);

      _packageVersionOption = new Option<NuGetVersion?>(["--version", "-v"], ParseVersion, description: "The version of the package you want to analyze.");
      AddOption(_packageVersionOption);

      _packageSourceOption = new Option<Uri?>(["--source", "-s"], ParseUri, description: "The url of the package source to use.");
      AddOption(_packageSourceOption);

      this.SetHandler(HandleCommand, _packageNameArgument, _packageVersionOption, _packageSourceOption);
    }

    private static NuGetVersion? ParseVersion(ArgumentResult result)
    {
      if (!result.Tokens.Any())
        return null;

      if (NuGetVersion.TryParse(result.Tokens.Single().Value, out var version))
        return version;
      else
      {
        result.ErrorMessage = $"'{result.Tokens.Single().Value}' is no valid package version.";
        return null; // Ignored.
      }
    }

    private static Uri? ParseUri(ArgumentResult result)
    {
      if (!result.Tokens.Any())
        return null;

      if (Uri.TryCreate(result.Tokens.Single().Value, UriKind.Absolute, out var uri))
        return uri;
      else
      {
        result.ErrorMessage = $"'{result.Tokens.Single().Value}' is no valid URI string.";
        return null; // Ignored.
      }
    }

    private void HandleCommand(string packageName, NuGetVersion? packageVersion, Uri? packageSource)
    {
      Console.WriteLine(packageName);
      Console.WriteLine(packageVersion);
      Console.WriteLine(packageSource);
    }
  }
}
