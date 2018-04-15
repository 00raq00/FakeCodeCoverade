using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeProjectToCover
{
  class Class1
  {
    private string _s;

    Class1()
    {
      s = "ss";
    }
    Class1(string s)
    {
      s2 = s;
    }
    Class1(List<string> sLists)
    {
      this.sList = sList;
    }
    string s
    {
      get;
      set;
    }
    string s1
    {
      get { return _s; }
      set { _s = value; }
    }

    string s2 { get; set; }
    
    List<string> sList { get; set; }
    
  }
}
