using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectToCover
{
  class Class4<T>:Class2
  {
    int i;
    double d;
    float f;
    decimal dd;
    T t1;
    public Class4(T t)
    {
      t1 = t;
    }
    public Class4(int i)
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
