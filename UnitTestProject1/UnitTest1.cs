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
      unit.RunCovererOnAssembly("multiImplementation");
      var errors = unit.GetErrors();

      unit.RunCovererOnAssembly("ProjectToCover");
      errors = unit.GetErrors();
    }
  }
}
