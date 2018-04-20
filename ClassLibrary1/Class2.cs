using multiImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
  class Class2
  {

    public Class2(AbsClass inter, AbsClass inter2)
    {
      if (inter == null)
        return;

      if (inter2 == null)
        return;

      if (inter is Class1)
      {
        Console.Write("");
      }

      if (inter is Class7)
      {
        Console.Write("");
      }
      if (inter is Class3)
      {
        Console.Write("");
      }
      if (inter is Class4)
      {
        Console.Write("");
      }

    }
    public void WriteConsole(AbsClass inter, AbsClass inter2)
    {
      if (inter == null)
        return;

      if (inter2 == null)
        return;

      if (inter is Class3)
      {
        Console.Write("");
      }
      if (inter is Class4)
      {
        Console.Write("");
      }

      if (inter is Class1)
      {
        Console.Write("");
      }

      if (inter is Class7)
      {
        Console.Write("");
      }
    }
  }
}
