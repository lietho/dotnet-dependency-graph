# About

[![Nuget](https://img.shields.io/nuget/v/DependencyGraph.App.svg)](https://www.nuget.org/packages/DependencyGraph.App/)

Allows you to create dependency graphs for .NET SDK-style projects. This is the .NET Global Tool allowing usage of dependency-graph from the command line. The most notable difference to other tools providing such functionality is that dotnet-dependency-graph also includes transitive dependencies, thus creating a complete dependency graph.

## Usage

### Installation

```powershell
dotnet tool install --global DependencyGraph.App
```

After installation, the tool can be called with the command `dependency-graph`. Currently the tool supports two different command: `print` and `trace`. 

### Print dependencies to the console

The `print` command allows you to create a dependency graph for a project or a whole solution and print it to the console or to a dgml file. Per default, dotnet-dependecy-graph will look for a project or solution file in the current directory, restore the project and print its dependencies to the console. For example, the call of

```powershell
dependency-graph print
```

for the project `DependencyGraph.App` will print the following output to the console

```
DependencyGraph.App
   DependencyGraph.App/net9.0
      DependencyGraph.Core
         Microsoft.Build@17.13.9
            Microsoft.Build.Framework@17.13.9
            Microsoft.NET.StringTools@17.13.9
            Microsoft.VisualStudio.SolutionPersistence@1.0.28
            System.Collections.Immutable@8.0.0
            System.Configuration.ConfigurationManager@8.0.0
               System.Diagnostics.EventLog@9.0.2
               System.Security.Cryptography.ProtectedData@8.0.0
            System.Reflection.Metadata@8.0.0
               System.Collections.Immutable@8.0.0
            System.Reflection.MetadataLoadContext@8.0.0
               System.Collections.Immutable@8.0.0
               System.Reflection.Metadata@8.0.0
                  System.Collections.Immutable@8.0.0
         NuGet.ProjectModel@6.13.1
            NuGet.DependencyResolver.Core@6.13.1
               NuGet.Configuration@6.13.1
                  NuGet.Common@6.13.1
                     NuGet.Frameworks@6.13.1
                     System.Collections.Immutable@8.0.0
                  System.Security.Cryptography.ProtectedData@8.0.0
               NuGet.LibraryModel@6.13.1
...
```

You can also explicitly specify a project file:

```powershell
dependency-graph print .\DependencyGraph.App\DependencyGraph.App.csproj
```

#### Use the DGML visualizer

Printing large dependency graphs to the console can be hard to comprehend. A better option may be to use the DGML visualizer. It creates a DGML file that can be viewed in Visual Studios DGML viewer. To use the DGML visualizer use the `-v dgml` or `--visualizer dgml` option. In this case, you must also specify an output file. The command

```powershell
dependency-graph print .\DependencyGraph.App\DependencyGraph.App.csproj -v dgml -o DependencyGraph.App.dgml
```

creates a dependency graph for the `DependencyGraph.App` project and writes it to the file `DependencyGraph.App.dgml`. The result may look like the following image:

![Dependency graph of the DependencyGraph.App visualized using the DGML visualizer](../../docs/DependencyGraph.App.svg "Dependency graph of the DependencyGraph.App project")

#### Exclude dependencies

Sometimes dependency graphs can get quite big and confusing. With the option `-e` or `--exclude` you can exclude dependencies from being added to the resulting dependency graph. For example, the command

```powershell
dependency-graph print -e Microsoft.* System.*
```

will exclude all dependencies that start with `Microsoft.` or `System.` from the resulting graph.

#### Explicitly include dependencies

Another possibility to create smaller and clearer dependency graphs is to use the `-i` or `--include` option. It allows to specify patterns for dependencies that should be included in the graph. All other dependencies will be omitted. For example, if you want to find out your dependencies on packages from your company, you can use the following command:

```powershell
dependency-graph print -i YourCompany.*
```

#### Limit graph depth

You can limit the graph depth by specifying the option `-d` or `--max-depth`. For example, the command

```powershell
dependency-graph print -d 2
```

will limit the depth of the resulting graph to 2.

### Trace a dependency in the graph

Sometimes it can be useful to find out how a given dependency comes into your project, e.g., if it has a vulnerability. For this purpose the `trace` command was added in version 2 of the tool. It creates a full dependency graph of the specified project or solution and prints all paths within this graph to the specified dependency. For example, the command

```powershell
dependency-graph trace .\DependencyGraph.App\DependencyGraph.App.csproj Newtonsoft.Json
```

prints all paths to the dependency `Newtonsoft.Json` to the console:

```
Found 2 paths to the specified dependency:
DependencyGraph.App -> DependencyGraph.App/net9.0 -> DependencyGraph.Core -> NuGet.ProjectModel@6.13.1 -> NuGet.DependencyResolver.Core@6.13.1 -> NuGet.Protocol@6.13.1 -> NuGet.Packaging@6.13.1 -> Newtonsoft.Json@13.0.3
DependencyGraph.App -> DependencyGraph.App/net9.0 -> NuGet.ProjectModel@6.13.1 -> NuGet.DependencyResolver.Core@6.13.1 -> NuGet.Protocol@6.13.1 -> NuGet.Packaging@6.13.1 -> Newtonsoft.Json@13.0.3
```

The dependency can also be specified as pattern. For example,

```powershell
dependency-graph trace .\DependencyGraph.App\DependencyGraph.App.csproj Microsoft.Build**
```

prints all paths to dependencies starting with `Microsoft.Build`.

#### Specifying the version

If you want to trace a specific version or a version range of a dependency, you can do so by specifying the `-v` or `--version` option. The option takes a `VersionRange` as value. More details about how such ranges can be specified can be found [here](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning?tabs=semver20sort#version-ranges). 

### Show help

```powershell
dependency-graph -h
```
