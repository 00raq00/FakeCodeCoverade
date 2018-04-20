using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectToCover
{
  class Class2
  {
    string s { get; set; }

    string s2 { get; set; }

    List<string> sList { get; set; }

    void Class2AddToList(string s)
    {
      sList = sList !=null? sList: new List<string>();
      sList.Add(s);
    }
  }
}
