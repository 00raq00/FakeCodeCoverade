using System;
using FakeCodeCoverade;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestMethod()
    {
      UnitTestFakeCoverer unit = new UnitTestFakeCoverer();
      unit.RunCovererOnAssembly("ProjectToCover");
      var errors = unit.GetErrors();
    }
  }
}
