using ClassLibrary1;
using multiImplementation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
  public class Class8 : DbConnection
  {
    int i;
    double d;
    float f;
    decimal dd;

    public Class8()
    {
    }

    public override string ConnectionString { get; set; }

    public override string Database { get;  }

    public override string DataSource { get;  }

    public override string ServerVersion { get;  }

    public override ConnectionState State { get;  }

    public override void ChangeDatabase(string databaseName)
    {
      
    }

    public override void Close()
    {
    
    }

    public override void Open()
    {
      
    }

    public void WriteConsole(Interface1 inter, Interface1 inter2,Class5_1 cs, object obj)
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

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      return null;
    }

    protected override DbCommand CreateDbCommand()
    {
      return null;
    }
  }
}
