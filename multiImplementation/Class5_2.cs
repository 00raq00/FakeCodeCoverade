using ProjectToCover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeProjectToCover
{
public  class Class5_2 : Interface1
  {
    int i;
    double d;
    float f;
    decimal dd;
  
    public Class5_2(int i)
    {
      this.i = i;
    }
    public void set(int i, double d, float f, decimal dd)
    {
      i = i;
      d = d;
      f = f;
      dd = dd;
    }
  }
}
