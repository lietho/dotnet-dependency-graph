# About

[![Nuget](https://img.shields.io/nuget/v/DependencyGraph.Core.svg)](https://www.nuget.org/packages/DependencyGraph.Core/)

Allows you to create dependency graphs for .NET SDK-style projects. The most notable difference to other libraries providing such functionality is that dotnet-dependency-graph also includes transitive dependencies, thus creating a complete dependency graph.

## Usage

```csharp
// read project lock file (project.assets.json)
var lockFileFormat = new LockFileFormat();
var lockFile = lockFileFormat.Read("project.assets.json", NullLogger.Instance);

// create dependency graph with default options
var dependencyGraphFactory = new DependencyGraphFactory();
var graph = dependencyGraphFactory.FromLockFile(lockFile);

// print dependency graph to the console
IDependencyGraphVisualizer visualizer = new ConsoleDependencyGraphVisualizer(new ConsoleDependencyGraphVisualizerOptions());
await visualizer.VisualizeAsync(graph);

// create DGML file for the dependency graph
visualizer = new DgmlDependencyGraphVisualizer(new DgmlDependencyGraphVisualizerOptions("graph.dgml"));
await visualizer.VisualizeAsync(graph);

// exclude dependencies starting with "Microsoft" or "System"
dependencyGraphFactory = new DependencyGraphFactory(new DependencyGraphFactoryOptions { Excludes = ["Microsoft.*", "System.*"] });
graph = dependencyGraphFactory.FromLockFile(lockFile);

// only include dependencies starting with "YourCompany"
dependencyGraphFactory = new DependencyGraphFactory(new DependencyGraphFactoryOptions { Includes = ["YourCompany.*"] });
graph = dependencyGraphFactory.FromLockFile(lockFile);

// limit the graph depth
dependencyGraphFactory = new DependencyGraphFactory(new DependencyGraphFactoryOptions { MaxDepth = 2 });
graph = dependencyGraphFactory.FromLockFile(lockFile);

// modify console visualizer options
var consoleVisualizerOptions = new ConsoleDependencyGraphVisualizerOptions
{
  IndentSize = 4, // specifiy indentation
  Renderer = graphNode => graphNode.ToString(), // modify how the text for a graph node gets rendered
  GetForegroundColor = // modify foregorund color
    (graphNode, currentColor) => graphNode switch
    {
      ProjectDependencyGraphNode => ConsoleColor.Cyan,
      PackageDependencyGraphNode => ConsoleColor.DarkMagenta,
      _ => currentColor
    },
  GetBackgroundColor = // modify background color
    (graphNode, currentColor) => currentColor
};

// modify DGML visualizer options
var dgmlVisualizerOptions = new DgmlDependencyGraphVisualizerOptions("graph.dgml")
{
  Categories = // specify categories for the graph nodes
    [
      new DirectedGraphCategory
      {
        Id = "DefaultCategory",
        Stroke = "Black",
        Background = "DarkOrange"
      }
    ],
  AssignCategory = graphNode => "DefaultCategory" // assign a node to a category
};
```

For example outputs please see [README.md](../DependencyGraph.App/README.md) of the dotnet tool.
