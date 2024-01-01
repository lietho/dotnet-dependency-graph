# About

Allows you to create dependency graphs for .NET SDK-style projects. The most notable difference to other libraries providing such functionality is that dotnet-dependency-graph also includes transitive dependencies, thus creating a complete dependency graph.

## Usage

```csharp
// read project lock file (project.assets.json)
var lockFileFormat = new LockFileFormat();
var lockFile = lockFileFormat.Read("project.assets.json", NullLogger.Instance);

// create dependency graph
var dependencyGraphFactory = new DependencyGraphFactory();
var graph = dependencyGraphFactory.FromLockFile(lockFile);

// print dependency graph to the console
var visualizer = new ConsoleDependencyGraphVisualizer(new ConsoleDependencyGraphVisualizerOptions());
await visualizer.VisualizeAsync(graph);

// create DGML file for the dependency graph
visualizer = new DgmlDependencyGraphVisualizer(new DgmlDependencyGraphVisualizerOptions { OutputFilePath = "graph.dgml" })
await visualizer.VisualizeAsync(graph);

// exclude dependencies starting with "Microsoft" or "System"
graph = dependencyGraphFactory.FromLockFile(lockFile, excludes: ["Microsoft.*", "System.*"]);

// only include dependencies starting with "YourCompany"
graph = dependencyGraphFactory.FromLockFile(lockFile, includes: ["YourCompany.*"]);

// limit the graph depth
graph = dependencyGraphFactory.FromLockFile(lockFile, maxDepth: 2);

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
var dgmlVisualizerOptions = new DgmlDependencyGraphVisualizerOptions
{
  OutputFilePath = "graph.dgml", // specify output file
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
