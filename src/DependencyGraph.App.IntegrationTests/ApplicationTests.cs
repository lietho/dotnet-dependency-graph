// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NUnit.Framework;

namespace DependencyGraph.App.IntegrationTests
{
  public class ApplicationTests
  {
    public static IEnumerable<TestCaseData> DgmlTestCases
    {
      get
      {
        yield return new TestCaseData("WebApplication.csproj.dgml", new[] { "print", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "dgml", "-o", "graph.dgml", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_Exclude.dgml", new[] { "print", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "dgml", "-o", "graph.dgml", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_Include.dgml", new[] { "print", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "dgml", "-o", "graph.dgml", "-i", "ClassLibrary*", "*Mapper*", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_MaxDepth.dgml", new[] { "print", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "dgml", "-o", "graph.dgml", "-d", "2", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln.dgml", new[] { "print", "../../TestFiles/TestSolution/TestSolution.sln", "-v", "dgml", "-o", "graph.dgml", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_Exclude.dgml", new[] { "print", "../../TestFiles/TestSolution/TestSolution.sln", "-v", "dgml", "-o", "graph.dgml", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_Include.dgml", new[] { "print", "../../TestFiles/TestSolution/TestSolution.sln", "-v", "dgml", "-o", "graph.dgml", "-i", "ClassLibrary*", "*Mapper*", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_MaxDepth.dgml", new[] { "print", "../../TestFiles/TestSolution/TestSolution.sln", "-v", "dgml", "-o", "graph.dgml", "-d", "2", "--no-restore" });
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
        yield return new TestCaseData("WebApplication.csproj.txt", new[] { "print", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "console", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_Exclude.txt", new[] { "print", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "console", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_Include.txt", new[] { "print", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "console", "-i", "ClassLibrary*", "*Mapper*", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_MaxDepth.txt", new[] { "print", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "-v", "console", "-d", "2", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln.txt", new[] { "print", "../../TestFiles/TestSolution/TestSolution.sln", "-v", "console", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_Exclude.txt", new[] { "print", "../../TestFiles/TestSolution/TestSolution.sln", "-v", "console", "-e", "Microsoft.Extensions*", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_Include.txt", new[] { "print", "../../TestFiles/TestSolution/TestSolution.sln", "-v", "console", "-i", "ClassLibrary*", "*Mapper*", "--no-restore" });

        yield return new TestCaseData("TestSolution.sln_MaxDepth.txt", new[] { "print", "../../TestFiles/TestSolution/TestSolution.sln", "-v", "console", "-d", "2", "--no-restore" });
      }
    }

    [Test, TestCaseSource(nameof(ConsoleTestCases))]
    public async Task ConsoleVisualizerTest(string testFileName, string[] args)
    {
      await RunAndCaptureOutput(args);

      CompareFiles("consoleOutput.txt", $"TestFiles/{testFileName}");
    }

    public static IEnumerable<TestCaseData> TraceTestCases
    {
      get
      {
        yield return new TestCaseData("WebApplication.csproj_Trace_Pattern.txt", new[] { "trace", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "System.Runtime.CompilerServices*", "--no-restore" });

        yield return new TestCaseData("ClassLibrary.csproj_Trace.txt", new[] { "trace", "../../TestFiles/TestSolution/ClassLibrary/ClassLibrary.csproj", "AutoMapper", "--no-restore" });

        yield return new TestCaseData("WebApplication.csproj_Trace_MinVersion.txt", new[] { "trace", "../../TestFiles/TestSolution/WebApplication/WebApplication.csproj", "AutoMapper", "-v", "12.0", "--no-restore" });
      }
    }

    [Test, TestCaseSource(nameof(TraceTestCases))]
    public async Task TraceTest(string testFileName, string[] args)
    {
      await RunAndCaptureOutput(args);

      CompareFiles("consoleOutput.txt", $"TestFiles/{testFileName}");
    }

    private static async Task RunAndCaptureOutput(string[] args)
    {
      var consoleOut = Console.Out;

      try
      {
        using var streamWriter = new StreamWriter("consoleOutput.txt", append: false);
        Console.SetOut(streamWriter);

        await Program.Main(args);
      }
      finally
      {
        Console.SetOut(consoleOut);
      }
    }

    private static void CompareFiles(string file, string expectedContentFile) =>
      Assert.That(NormalizeNewLines(File.ReadAllText(file)), Is.EqualTo(NormalizeNewLines(File.ReadAllText(expectedContentFile))));

    private static string NormalizeNewLines(string text) => text.Replace("\r\n", Environment.NewLine);
  }
}
