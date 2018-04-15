using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeProjectToCover
{
  class Class3<T>:Class2 where T:Class1
  {
    int i;
    double d;
    float f;
    decimal dd;



    public void set(int i, double d, float f, decimal dd)
    {
      i = i;
      d = d;
      f = f;
      dd = dd;
    }
  }
}
