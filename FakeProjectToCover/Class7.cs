﻿using multiImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectToCover
{
  class Class7
  {
    int i;
    double d;
    float f;
    decimal dd;

    public Class7(Interface1 inter)
    {
      if (inter is Class5)
      {
        Console.Write("");
      }

      if (inter is Class5_1)
      {
        Console.Write("");
      }

      if (inter is Class5_2)
      {
        Console.Write("");
      }

    }
    public void WriteConsole(Interface1 inter)
    {
      if (inter is Class5)
      {
        Console.Write("");
      }

      if (inter is Class5_1)
      {
        Console.Write("");
      }

      if (inter is Class5_2)
      {
        Console.Write("");
      }

    }

  }
}
