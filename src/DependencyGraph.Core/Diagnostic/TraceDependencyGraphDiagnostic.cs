// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using DependencyGraph.Core.Graph;
using NuGet.Versioning;

namespace DependencyGraph.Core.Diagnostic
{
  public class TraceDependencyGraphDiagnostic : GraphDiagnosticBase<IEnumerable<IImmutableList<IDependencyGraphNode>>>
  {
    private readonly string _namePattern;
    private readonly VersionRange? _versionRange;

    public TraceDependencyGraphDiagnostic(string namePattern, VersionRange? versionRange)
    {
      _namePattern = namePattern;
      _versionRange = versionRange;
    }

    public override IEnumerable<IImmutableList<IDependencyGraphNode>> Perform(IDependencyGraph dependencyGraph)
       => TraverseDepthFirst(dependencyGraph).Where(PathEndsWithSearchedDependency);

    private bool PathEndsWithSearchedDependency(IImmutableList<IDependencyGraphNode> path)
    {
      var lastNode = path[path.Count - 1];

      if (_versionRange == null)
        return Regex.IsMatch(lastNode.Name, _namePattern, RegexOptions.IgnoreCase);
      else if (lastNode is PackageDependencyGraphNode packageDependencyGraphNode)
        return Regex.IsMatch(packageDependencyGraphNode.Name, _namePattern, RegexOptions.IgnoreCase) && _versionRange.Satisfies(packageDependencyGraphNode.Version, VersionComparison.Version);

      return false;
    }
  }
}
