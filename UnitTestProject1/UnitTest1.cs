using System;
using System.Linq;
using AutoCodeCoverade;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using multiImplementation;

namespace UnitTestProject1
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestMethod()
    {
      AutoCoverOptions autoCoverOptions = new AutoCoverOptions()
      {
        AllowSearchInMicrosoftAssembly = false,
        CoverFields = false,
        CoverProperties = true,
        InvokeMethods = true,
        MaxDegreeOfParallelismForCreateInstances = 2,
        MaxDegreeOfParallelismForCombinationOfParametersMethodInvokes = 4,
        MaxDegreeOfParallelismForMethodsInvokes = 4,
        SearchImpelentationInSourceAssembly = true,
        TryCoverBaseExternal = false
      };
      AutoCodeCoverer unit = new AutoCodeCoverer(autoCoverOptions);
      unit.SetInstanceToInject(typeof(Interface1), new Class5(9));
      unit.SetInstanceToInject(typeof(object), new Class7(null,null));
      unit.RunCovererOnAssembly("ClassLibrary1", "multiImplementation", "ProjectToCover");
      var errors = unit.GetErrors();

      Assert.AreEqual(errors.Count(), 0);
    }
  }
}
