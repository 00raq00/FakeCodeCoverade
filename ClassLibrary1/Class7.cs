using ClassLibrary1;
using multiImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
  class Class7:AbsClass
  {
    int i;
    double d;
    float f;
    decimal dd;

    public Class7(Interface1 inter, Interface1 inter2, object obj)
    {
      if (inter == null)
        return;

      if (inter2 == null)
        return;

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


      if (obj == null)
        return;

      if (obj is Class5)
      {
        Console.Write("");
      }

      if (obj is Class5_1)
      {
        Console.Write("");
      }

      if (obj is Class5_2)
      {
        Console.Write("");
      }
      if (obj is Class1)
      {
        Console.Write("");
      }
      if (obj is Class2)
      {
        Console.Write("");
      }
      if (obj is Class3)
      {
        Console.Write("");
      }
      if (obj is Class4)
      {
        Console.Write("");
      }
      if (obj is multiImplementation.Class7)
      {
        Console.Write("");
      }
      

    }
    public void WriteConsole(Interface1 inter, Interface1 inter2,Class5_1 cs,object obj)
    {
      if (inter == null)
        return;

      if (inter2 == null)
        return;

   
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



      if (obj is Class5_2)
      {
        Console.Write("");
      }
      if (obj is Class1)
      {
        Console.Write("");
      }
      if (obj is Class2)
      {
        Console.Write("");
      }
      if (obj is Class3)
      {
        Console.Write("");
      }
      if (obj is Class4)
      {
        Console.Write("");
      }
      
      if (obj is multiImplementation.Class7)
      {
        Console.Write("");
      }
    }

  }
}
