// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using DependencyGraph.App;
using NUnit.Framework;

namespace DependencyGraph.App.IntegrationTests
{
  public class DependencyGraphRootCommandTests
  {
    [OneTimeSetUp]
    public static void OnTimeSetUp()
    {
      var process = Process.Start("dotnet", "restore ../../TestFiles/TestSolution");
      process.WaitForExit();

      Assert.That(process.ExitCode, Is.EqualTo(0), "Restore failed.");
    }

    public static IEnumerable<TestCaseData> DgmlTestCases
    {
      get
      {
        yield return new TestCaseData("WebApplication.csproj.dgml", new[] { @"../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "dgml", "-o", "graph.dgml", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_Exclude.dgml", new[] { @"../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "dgml", "-o", "graph.dgml", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_Include.dgml", new[] { @"../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "dgml", "-o", "graph.dgml", "-i", "ClassLibrary*", "*Mapper*", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_MaxDepth.dgml", new[] { @"../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "dgml", "-o", "graph.dgml", "-d", "2", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln.dgml", new[] { @"../../TestFiles/TestSolution/TestSolution.sln", "-v", "dgml", "-o", "graph.dgml", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_Exclude.dgml", new[] { @"../../TestFiles/TestSolution/TestSolution.sln", "-v", "dgml", "-o", "graph.dgml", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_Include.dgml", new[] { @"../../TestFiles/TestSolution/TestSolution.sln", "-v", "dgml", "-o", "graph.dgml", "-i", "ClassLibrary*", "*Mapper*", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_MaxDepth.dgml", new[] { @"../../TestFiles/TestSolution/TestSolution.sln", "-v", "dgml", "-o", "graph.dgml", "-d", "2", "--no-restore" });
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
        yield return new TestCaseData("WebApplication.csproj.txt", new[] { @"../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "console", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_Exclude.txt", new[] { @"../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "console", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_Include.txt", new[] { @"../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "console", "-i", "ClassLibrary*", "*Mapper*", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_MaxDepth.txt", new[] { @"../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "console", "-d", "2", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln.txt", new[] { @"../../TestFiles/TestSolution/TestSolution.sln", "-v", "console", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_Exclude.txt", new[] { @"../../TestFiles/TestSolution/TestSolution.sln", "-v", "console", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_Include.txt", new[] { @"../../TestFiles/TestSolution/TestSolution.sln", "-v", "console", "-i", "ClassLibrary*", "*Mapper*", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_MaxDepth.txt", new[] { @"../../TestFiles/TestSolution/TestSolution.sln", "-v", "console", "-d", "2", "--no-restore" });
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
