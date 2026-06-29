# dotnet-dependency-graph
[![.NET build and test](https://github.com/lietho/dotnet-dependency-graph/actions/workflows/CI.yml/badge.svg)](https://github.com/lietho/dotnet-dependency-graph/actions/workflows/CI.yml)

dotnet-dependency-graph is a dotnet global tool and library that helps you analyze the dependencies of .NET SDK-style projects. The most notable difference to other tools providing such functionality is that dotnet-dependency-graph also includes transitive dependencies, thus creating a complete dependency graph.

In addition to printing and tracing dependencies from the command line, the tool can run as a [Model Context Protocol](https://modelcontextprotocol.io/) (MCP) server (`dependency-graph mcp`), making the dependency graph available to AI agents. See the [tool documentation](./src/DependencyGraph.App/README.md#provide-dependency-information-to-ai-agents-mcp-server) for details.

This repository contains the dotnet global tool [DependencyGraph.App](./src/DependencyGraph.App/README.md) and the core library [DependencyGraph.Core](./src/DependencyCore.App/README.md).