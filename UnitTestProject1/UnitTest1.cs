using System;
using System.Linq;
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
      UnitTestFakeCoverer unit = new UnitTestFakeCoverer(searchImpelentationInSourceAssembly: true, searchInSystemAssembly: false);
      unit.SetMaxParalelism(2);
      unit.RunCovererOnAssembly("ClassLibrary1", "multiImplementation", "ProjectToCover");
      var errors = unit.GetErrors();

      Assert.AreEqual(errors.Count(), 0);
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
