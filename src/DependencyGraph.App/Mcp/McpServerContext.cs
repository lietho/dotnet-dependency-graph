// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;

namespace DependencyGraph.App.Mcp
{
  /// <summary>
  /// Holds server-wide state for the MCP server, in particular the default project or solution file that the
  /// tools analyze when no explicit path is supplied on a tool call.
  /// </summary>
  public sealed class McpServerContext
  {
    public McpServerContext(FileInfo? defaultProjectOrSolutionFile)
    {
      DefaultProjectOrSolutionFile = defaultProjectOrSolutionFile;
    }

    /// <summary>
    /// The project or solution file the server was started with (or the single one found in the working
    /// directory). May be <c>null</c> if neither was available; in that case tool calls must specify a path.
    /// </summary>
    public FileInfo? DefaultProjectOrSolutionFile { get; }
  }
}
