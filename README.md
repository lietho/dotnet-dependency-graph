# dotnet-dependency-graph
[![.NET build and test](https://github.com/lietho/dotnet-dependency-graph/actions/workflows/dotnet.yml/badge.svg)](https://github.com/lietho/dotnet-dependency-graph/actions/workflows/dotnet.yml)

dotnet-dependency-graph is a dotnet global tool and library that helps you analyze the dependencies of .NET SDK-style projects. The most notable difference to other tools providing such functionality is that dotnet-dependency-graph also includes transitive dependencies, thus creating a complete dependency graph.

This repository contains the dotnet global tool [DependencyGraph.App](./src/DependencyGraph.App/README.md) and the core library [DependencyGraph.Core](./src/DependencyCore.App/README.md).