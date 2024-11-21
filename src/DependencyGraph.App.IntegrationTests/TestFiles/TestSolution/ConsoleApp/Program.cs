using System;
using ClassLibrary;

namespace ConsoleApp
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      var calculator = new Calculator();

      Console.WriteLine(calculator.Add(1, 2));
    }
  }
}
