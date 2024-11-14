// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using NuGet.ProjectModel;
using NuGet.Versioning;

namespace DependencyGraph.Core.Graph
{
  public class PackageDependencyGraphNode : DependencyGraphNode, IEquatable<PackageDependencyGraphNode>
  {
    public PackageDependencyGraphNode(string name, NuGetVersion version, LockFileTargetLibrary targetLibrary) : base(name)
    {
      Version = version;
      TargetLibrary = targetLibrary;
    }

    public NuGetVersion Version { get; }
    public LockFileTargetLibrary TargetLibrary { get; }

    public override string ToString() => $"{Name}@{Version}";

    public override bool Equals(object? obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      return Equals((PackageDependencyGraphNode)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return 13 * StringComparer.OrdinalIgnoreCase.GetHashCode(Name) ^ 23 * Version.GetHashCode();
      }
    }

    public bool Equals(PackageDependencyGraphNode? other) => base.Equals(other) && Version.Equals(other?.Version);
  }
}
