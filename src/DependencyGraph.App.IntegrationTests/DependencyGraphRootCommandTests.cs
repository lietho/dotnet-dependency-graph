// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DependencyGraph.App.IntegrationTests
{
  public class DependencyGraphRootCommandTests
  {
    public static IEnumerable<TestCaseData> DgmlTestCases
    {
      get
      {
        yield return new TestCaseData("DependencyGraph.App.csproj.dgml", new[] { @"../../../DependencyGraph.App/DependencyGraph.App.csproj", "-v", "dgml", "-o", "graph.dgml", "--no-restore" });

        yield return new TestCaseData("DependencyGraph.App.csproj_Exclude.dgml", new[] { @"../../../DependencyGraph.App/DependencyGraph.App.csproj", "-v", "dgml", "-o", "graph.dgml", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("DependencyGraph.App.csproj_Include.dgml", new[] { @"../../../DependencyGraph.App/DependencyGraph.App.csproj", "-v", "dgml", "-o", "graph.dgml", "-i", "DependencyGraph*", "*NuGet*", "--no-restore" });

        yield return new TestCaseData("DependencyGraph.App.csproj_MaxDepth.dgml", new[] { @"../../../DependencyGraph.App/DependencyGraph.App.csproj", "-v", "dgml", "-o", "graph.dgml", "-d", "2", "--no-restore" });
      }
    }

    [Test, TestCaseSource(nameof(DgmlTestCases))]
    public async Task DgmlVisualizerTest(string testFileName, string[] args)
    {
      await Program.Main(args);

      CompareFiles("graph.dgml", $"TestFiles/{testFileName}");
    }

    public static IEnumerable<TestCaseData> ConsoleTestCases
    {
      get
      {
        yield return new TestCaseData("DependencyGraph.App.csproj.txt", new[] { @"../../../DependencyGraph.App/DependencyGraph.App.csproj", "-v", "console", "--no-restore" });

        yield return new TestCaseData("DependencyGraph.App.csproj_Exclude.txt", new[] { @"../../../DependencyGraph.App/DependencyGraph.App.csproj", "-v", "console", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("DependencyGraph.App.csproj_Include.txt", new[] { @"../../../DependencyGraph.App/DependencyGraph.App.csproj", "-v", "console", "-i", "DependencyGraph*", "*NuGet*", "--no-restore" });

        yield return new TestCaseData("DependencyGraph.App.csproj_MaxDepth.txt", new[] { @"../../../DependencyGraph.App/DependencyGraph.App.csproj", "-v", "console", "-d", "2", "--no-restore" });
      }
    }

    [Test, TestCaseSource(nameof(ConsoleTestCases))]
    public async Task ConsoleVisualizerTest(string testFileName, string[] args)
    {
      var consoleOut = Console.Out;

      try
      {
        using (var streamWriter = new StreamWriter("consoleOutput.txt", append: false))
        {
          Console.SetOut(streamWriter);

          await Program.Main(args);
        }
      }
      finally
      {
        Console.SetOut(consoleOut);
      }

      CompareFiles("consoleOutput.txt", $"TestFiles/{testFileName}");
    }

    private static void CompareFiles(string file, string expectedContentFile) => 
      Assert.That(NormalizeNewLines(File.ReadAllText(file)), Is.EqualTo(NormalizeNewLines(File.ReadAllText(expectedContentFile))));

    private static string NormalizeNewLines(string text) => text.Replace("\r\n", Environment.NewLine);
  }
}
