// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NUnit.Framework;

namespace ClassLibrary.UnitTests
{
  public class CalculatorTests
  {
    [Test]
    public void Add()
    {
      var calculator = new Calculator();

      Assert.That(calculator.Add(1, 2), Is.EqualTo(3));
    }
  }
}
