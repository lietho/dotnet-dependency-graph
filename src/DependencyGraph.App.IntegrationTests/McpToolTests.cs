// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using DependencyGraph.App.Mcp;
using NUnit.Framework;

namespace DependencyGraph.App.IntegrationTests
{
  public class McpToolTests
  {
    private const string ClassLibraryProject = "../../TestFiles/TestSolution/ClassLibrary/ClassLibrary.csproj";
    private const string WebApplicationProject = "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj";
    private const string TestSolution = "../../TestFiles/TestSolution/TestSolution.slnx";

    private static string AbsolutePath(string relativePath) => Path.GetFullPath(relativePath);

    /// <summary>Tools without a default file; every call must supply an explicit (absolute) path.</summary>
    private static DependencyGraphTools ToolsWithoutDefault() => new(new McpServerContext(null));

    /// <summary>Tools with a default file, mirroring a server started with a project/solution path.</summary>
    private static DependencyGraphTools ToolsWithDefault(string relativePath) => new(new McpServerContext(new FileInfo(AbsolutePath(relativePath))));

    [Test]
    public async Task GetDependencyGraph_ReturnsRootProjectWithTargetFrameworks()
    {
      var graph = await ToolsWithoutDefault().GetDependencyGraphAsync(projectOrSolutionFile: AbsolutePath(ClassLibraryProject), noRestore: true);

      Assert.That(graph.Description, Is.Not.Empty);
      Assert.That(graph.RootNodes, Has.Count.EqualTo(1));

      var root = graph.RootNodes[0];
      Assert.That(root.Kind, Is.EqualTo(DependencyNodeKind.RootProject));
      Assert.That(root.Name, Is.EqualTo("ClassLibrary"));
      Assert.That(root.Dependencies.Select(node => node.Kind), Is.All.EqualTo(DependencyNodeKind.TargetFramework));
      Assert.That(root.Dependencies.Select(node => node.TargetFramework),
        Is.EquivalentTo(new[] { ".NETFramework,Version=v4.7.2", "net8.0" }));
    }

    [Test]
    public async Task GetDependencyGraph_UsesDefaultFileWhenNoPathProvided()
    {
      var graph = await ToolsWithDefault(ClassLibraryProject).GetDependencyGraphAsync(noRestore: true);

      Assert.That(graph.RootNodes, Has.Count.EqualTo(1));
      Assert.That(graph.RootNodes[0].Name, Is.EqualTo("ClassLibrary"));
    }

    [Test]
    public void GetDependencyGraph_ThrowsWhenNoPathAndNoDefault()
    {
      Assert.That(
        async () => await ToolsWithoutDefault().GetDependencyGraphAsync(noRestore: true),
        Throws.ArgumentException);
    }

    [Test]
    public void GetDependencyGraph_RequiresAbsolutePath()
    {
      Assert.That(
        async () => await ToolsWithoutDefault().GetDependencyGraphAsync(projectOrSolutionFile: ClassLibraryProject, noRestore: true),
        Throws.ArgumentException);
    }

    [Test]
    public async Task TraceDependency_FindsAllPathsToPackage()
    {
      var result = await ToolsWithoutDefault().TraceDependencyAsync("AutoMapper", noRestore: true, projectOrSolutionFile: AbsolutePath(ClassLibraryProject));

      Assert.That(result.PathCount, Is.EqualTo(2));
      Assert.That(result.Paths, Has.Count.EqualTo(2));

      foreach (var path in result.Paths)
      {
        var last = path[^1];
        Assert.That(last.Kind, Is.EqualTo(DependencyNodeKind.Package));
        Assert.That(last.Name, Is.EqualTo("AutoMapper"));
        Assert.That(last.Version, Is.EqualTo("10.1.1"));
      }
    }

    [Test]
    public async Task TraceDependency_HonorsVersionRange()
    {
      var result = await ToolsWithoutDefault().TraceDependencyAsync("AutoMapper", versionRange: "12.0", noRestore: true, projectOrSolutionFile: AbsolutePath(WebApplicationProject));

      Assert.That(result.PathCount, Is.EqualTo(2));
      Assert.That(result.Paths.Select(path => path[^1].Version), Is.All.EqualTo("13.0.1"));
    }

    [Test]
    public void TraceDependency_ThrowsOnInvalidVersionRange()
    {
      Assert.That(
        async () => await ToolsWithoutDefault().TraceDependencyAsync("AutoMapper", versionRange: "not-a-version", noRestore: true, projectOrSolutionFile: AbsolutePath(ClassLibraryProject)),
        Throws.ArgumentException);
    }

    [Test]
    public async Task ListTargetFrameworks_ReturnsPerProjectFrameworks()
    {
      var frameworks = await ToolsWithoutDefault().ListTargetFrameworksAsync(noRestore: true, projectOrSolutionFile: AbsolutePath(ClassLibraryProject));

      Assert.That(frameworks, Is.EquivalentTo(new[]
      {
        "ClassLibrary/.NETFramework,Version=v4.7.2",
        "ClassLibrary/net8.0"
      }));
    }

    [Test]
    public async Task ListPackages_IncludesResolvedPackageVersions()
    {
      var packages = await ToolsWithoutDefault().ListPackagesAsync(noRestore: true, projectOrSolutionFile: AbsolutePath(ClassLibraryProject));

      Assert.That(packages, Has.One.Matches<PackageDto>(package => package.Name == "AutoMapper" && package.Version == "10.1.1"));
    }

    [Test]
    public async Task FindVersionConflicts_DetectsMultipleVersionsAcrossSolution()
    {
      var conflicts = await ToolsWithoutDefault().FindVersionConflictsAsync(noRestore: true, projectOrSolutionFile: AbsolutePath(TestSolution));

      var autoMapper = conflicts.SingleOrDefault(conflict => conflict.Name == "AutoMapper");
      Assert.That(autoMapper, Is.Not.Null);
      Assert.That(autoMapper!.Versions, Is.EquivalentTo(new[] { "10.1.1", "13.0.1" }));
    }
  }
}
