using ProjectToCover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeProjectToCover
{
  class Class7
  {
    int i;
    double d;
    float f;
    decimal dd;

    public Class7(Interface1 inter, Interface1 inter2)
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
