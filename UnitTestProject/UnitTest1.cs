﻿using System;
using System.Linq;
using AutoCodeCoverage;

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
        CoverFields = true,
        CoverProperties = true,
        InvokeMethods = true,
        MaxDegreeOfParallelismForCreateInstances = 10,
        MaxDegreeOfParallelismForCombinationOfParametersMethodInvokes = 10,
        MaxDegreeOfParallelismForMethodsInvokes = 10,
        SearchImplentationInSourceAssembly = true,
        TryCoverBaseExternal = false,
        TopParameterCombinationsForCreateInstanes = 10,
        TopParameterCombinationsForInvokeMethods = 10,
        AllowNullsAsConstractorParameter = true,
        AllowNullsAsMethodParameter = true,
        AllowRandomizeParametersWithTopCount=true
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
