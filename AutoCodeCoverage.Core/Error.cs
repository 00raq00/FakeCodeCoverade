using System;

namespace AutoCodeCoverage
{
  public class Error
  {
    public Exception Exception { get; internal set; }
    public Type Type { get; internal set; }
    public object[] Parameters { get; internal set; }
    public ErrorTypeEnum ErrorType { get; internal set; }
  }
}