using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCodeCoverade
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

    public bool SearchImpelentationInSourceAssembly { get => _searchImpelentationInSourceAssembly; set => _searchImpelentationInSourceAssembly = value; }
    public bool TryCoverBaseExternal { get => _tryCoverBaseExternal; set => _tryCoverBaseExternal = value; }
    public bool InvokeMethods { get => _invokeMethods; set => _invokeMethods = value; }
    public bool CoverProperties { get => _coverProperties; set => _coverProperties = value; }
    public bool CoverFields { get => _coverFields; set => _coverFields = value; }
    public bool AllowSearchInMicrosoftAssembly { get => _allowSearchInMicrosoftAssembly; set => _allowSearchInMicrosoftAssembly = value; }
    public int MaxDegreeOfParallelismForCreateInstances { get => _maxDegreeOfParallelismForCreateInstances; set => _maxDegreeOfParallelismForCreateInstances = value; }
    public int MaxDegreeOfParallelismForCombinationOfParametersMethodInvokes { get => _maxDegreeOfParallelismForCombinationOfParametersMethodInvokes; set => _maxDegreeOfParallelismForCombinationOfParametersMethodInvokes = value; }
    public int MaxDegreeOfParallelismForMethodsInvokes { get => _maxDegreeOfParallelismForMethodsInvokes; set => _maxDegreeOfParallelismForMethodsInvokes = value; }
  }
}
