﻿using multiImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectToCover
{
  class Class5:Interface1
  {
    int i;
    double d;
    float f;
    decimal dd;
  
    public Class5(int i)
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
