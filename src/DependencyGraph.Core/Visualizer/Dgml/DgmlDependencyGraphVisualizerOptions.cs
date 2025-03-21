﻿// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using DependencyGraph.Core.Graph;
using DependencyGraph.Core.Visualizer.Dgml.Models;
using System;
using System.Collections.Generic;

namespace DependencyGraph.Core.Visualizer.Dgml
{
  public class DgmlDependencyGraphVisualizerOptions
  {
    private const string DependencyId = "Dependency";
    private const string TargetFrameworkDependencyCategoryId = "FrameworkDependency";
    private const string ProjectDependencyCategoryId = "ProjectDependency";
    private const string PackageDependencyCategoryId = "PackageDependency";

    public DgmlDependencyGraphVisualizerOptions(string outputFilePath)
    {
      OutputFilePath = outputFilePath;
    }

    public string OutputFilePath { get; }

    public IList<DirectedGraphCategory> Categories { get; set; } =
      new List<DirectedGraphCategory>()
      {
        new DirectedGraphCategory
        {
          Id = DependencyId,
          Stroke = "Black"
        },
        new DirectedGraphCategory
        {
          Id = TargetFrameworkDependencyCategoryId,
          BasedOn = DependencyId,
          Background = "#FF502CCD"
        },
        new DirectedGraphCategory
        {
          Id = ProjectDependencyCategoryId,
          BasedOn = DependencyId,
          Background = "DarkOrange"
        },
        new DirectedGraphCategory
        {
          Id = PackageDependencyCategoryId,
          BasedOn = DependencyId,
          Background = "#FF06487D"
        }
      };

    public Func<IDependencyGraphNode, string?> AssignCategory { get; set; } = (node) => node switch
      {
        RootProjectDependencyGraphNode _ => ProjectDependencyCategoryId,
        TargetFrameworkDependencyGraphNode _ => TargetFrameworkDependencyCategoryId,
        ProjectDependencyGraphNode _ => ProjectDependencyCategoryId,
        PackageDependencyGraphNode _ => PackageDependencyCategoryId,
        _ => null
      };
  }
}
