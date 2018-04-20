using multiImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectToCover
{
  class Class6
  {
    int i;
    double d;
    float f;
    decimal dd;
  
    public Class6(Interface1 inter)
    {
      inte = inter;
    }

    public Interface1 inte { get; private set; }

    public void set(int i, double d, float f, decimal dd)
    {
      i = i;
      d = d;
      f = f;
      dd = dd;
    }
  }
}
