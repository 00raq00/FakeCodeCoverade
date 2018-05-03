using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCodeCoverage
{
  public class AutoCoverOptions
  {
    private bool _searchImpelentationInSourceAssembly = false;
    private bool _tryCoverBaseExternal = false;
    private bool _invokeMethods = true;
    private bool _coverProperties = true;
    private bool _coverFields = true;
    private bool _allowSearchInMicrosoftAssembly = false;
    private int _maxDegreeOfParallelismForCreateInstances = 1;
    private int _maxDegreeOfParallelismForCombinationOfParametersMethodInvokes = 1;
    private int _maxDegreeOfParallelismForMethodsInvokes = 1;
    private int _topParameterCombinationsForCreateInstances = 10;
    private int _topParameterCombinationsForInvokeMethods = 10;
    private bool _allowNullsAsMethodParameter = false;
    private bool _allowNullsAsConstractorParameter = false;
    private bool _allowRandomizeParametersWithTopCount;
    private int _topCountOfSameObjectInstances = 1;

    public bool SearchImplentationInSourceAssembly { get => _searchImpelentationInSourceAssembly; set => _searchImpelentationInSourceAssembly = value; }
    public bool TryCoverBaseExternal { get => _tryCoverBaseExternal; set => _tryCoverBaseExternal = value; }
    public bool InvokeMethods { get => _invokeMethods; set => _invokeMethods = value; }
    public bool CoverProperties { get => _coverProperties; set => _coverProperties = value; }
    public bool CoverFields { get => _coverFields; set => _coverFields = value; }
    public bool AllowSearchInMicrosoftAssembly { get => _allowSearchInMicrosoftAssembly; set => _allowSearchInMicrosoftAssembly = value; }
    public bool AllowNullsAsMethodParameter { get => _allowNullsAsMethodParameter; set => _allowNullsAsMethodParameter = value; }
    public bool AllowNullsAsConstractorParameter { get => _allowNullsAsConstractorParameter; set => _allowNullsAsConstractorParameter = value; }
    public int MaxDegreeOfParallelismForCreateInstances { get => _maxDegreeOfParallelismForCreateInstances; set => _maxDegreeOfParallelismForCreateInstances = value; }
    public int MaxDegreeOfParallelismForCombinationOfParametersMethodInvokes { get => _maxDegreeOfParallelismForCombinationOfParametersMethodInvokes; set => _maxDegreeOfParallelismForCombinationOfParametersMethodInvokes = value; }
    public int MaxDegreeOfParallelismForMethodsInvokes { get => _maxDegreeOfParallelismForMethodsInvokes; set => _maxDegreeOfParallelismForMethodsInvokes = value; }
    public int TopParameterCombinationsForInvokeMethods { get => _topParameterCombinationsForInvokeMethods; set => _topParameterCombinationsForInvokeMethods = value; }
    public int TopParameterCombinationsForCreateInstances { get => _topParameterCombinationsForCreateInstances; set => _topParameterCombinationsForCreateInstances = value; }
    public int TopCountOfSameObjectInstances { get => _topCountOfSameObjectInstances; set => _topCountOfSameObjectInstances = value; }
    //causes nondeterministic coverage
    public bool AllowRandomizeParametersWithTopCount { get => _allowRandomizeParametersWithTopCount; set => _allowRandomizeParametersWithTopCount = value; }
  }
}
