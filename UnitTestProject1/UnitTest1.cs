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
      UnitTestFakeCoverer unit = new UnitTestFakeCoverer(searchImpelentationInSourceAssembly: true, searchInSystemAssembly: true);
      unit.SetMaxParalelism(2);
      unit.RunCovererOnAssembly("ClassLibrary1", "multiImplementation");
      var errors = unit.GetErrors();

      /*
      UnitTestFakeCoverer unitWithOption = new UnitTestFakeCoverer(searchImpelentationInSourceAssembly: true);
      unitWithOption.RunCovererOnAssembly("ClassLibrary1", "multiImplementation");
      var errorsWithOption = unitWithOption.GetErrors();


      var unitWithOptionHighRisk = new UnitTestFakeCoverer(searchImpelentationInSourceAssembly: true,searchInSystemAssembly:true);
      unitWithOptionHighRisk.RunCovererOnAssembly("ClassLibrary1", "multiImplementation");
      var errorsWithOptionHighRisk = unitWithOptionHighRisk.GetErrors();
      */
    }
  }
}
