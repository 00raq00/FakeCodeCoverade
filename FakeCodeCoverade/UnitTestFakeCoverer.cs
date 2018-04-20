
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FakeCodeCoverade
{
  public  class UnitTestFakeCoverer
  {
    //todo: 
    //recognize abstract classes   !!DONE!!
    //multiple assembly support   !!DONE!!
    //cache and predefined implementations
    //parallel implementaion    !!DONE!!
    //refactor if necessary

    ConcurrentBag<Error> errorList = new ConcurrentBag<Error>();
    List<Assembly> assemblies=new List<Assembly>();
    private int _MaxParalelism = 1;

    public UnitTestFakeCoverer(bool searchImpelentationInSourceAssembly = false, bool searchInSystemAssembly = false)
    {
      SearchImpelentationInSourceAssembly = searchImpelentationInSourceAssembly;
      AllowSearchInMicrosoftAssembly = searchInSystemAssembly;
    }

    public bool SearchImpelentationInSourceAssembly { get; private set; }

    private readonly bool AllowSearchInMicrosoftAssembly;

    public void RunCovererOnAssembly(params string[] assemblyNames)
    {
      if (assemblyNames != null)
      {
        foreach (var assemblyName in assemblyNames)
        {
          try
          {
            var assembly = Assembly.Load(assemblyName);
            assemblies.Add(assembly);
          }
          catch (Exception e)
          {

          }
        }
        foreach (var assembly in assemblies)
        {
          foreach (var type in assembly.GetTypes())
          {
            var enumerable = CreateInstaceOfType(assembly, type, type).ToList();
            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = _MaxParalelism };
            Parallel.ForEach(enumerable, parallelOptions, inst =>
            {
              if (inst != null)
              {
                var properties = inst.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
                var methods = inst.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
                var members = inst.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
                var fields = inst.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

                foreach (var met in methods)
                {
                  RunMethods(type, inst, met);
                }

                foreach (var field in fields)
                {
                  GetAndSetValue(inst, field);
                }


                foreach (var property in properties)
                {
                  GetAndSetValue(inst, property);
                }
              }
            });

            enumerable = CreateInstaceOfType(assembly, type, type, true).ToList();
            Parallel.ForEach(enumerable, parallelOptions, inst =>
            {
              if (inst != null)
              {
                var properties = inst.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
                var methods = inst.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
                var members = inst.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
                var fields = inst.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

                foreach (var met in methods)
                {
                  RunMethods(type, inst, met);
                }

                foreach (var field in fields)
                {
                  GetAndSetValue(inst, field);
                }


                foreach (var property in properties)
                {
                  GetAndSetValue(inst, property);
                }
              }
            });
          }
        }
      }
    }

    public List<Error> GetErrors()
    {
      List<Error> list = errorList.ToList();
      return list;
    }

    private void RunMethods(Type type, object inst, MethodInfo met)
    {
      {
        object[] parameters = null;
        List<object> parametersList = new List<object>();
        foreach (var param in met.GetParameters())
        {
          Type paramType = param.ParameterType;
          parametersList.Add(GetDefault(paramType, paramType));
        }
        parameters = parametersList.ToArray();

        try
        {
          met.Invoke(inst, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null);
        }
        catch (Exception e)
        {
          errorList.Add(new Error() { Exception = e, Type = type, MethodName = "RunMethods-DefaultValues", Parameters = parameters });
        }
      }
      {
        object[] parameters = null;
        List<List<object>> parametersList = new List<List<object>>();
        foreach (var param in met.GetParameters())
        {
          Type t = param.ParameterType;
          List<object> item = GetDefaults(t, type,true).ToList();
          item.Add(null);
          parametersList.Add(item);
        }

        var tmpd = parametersList.Select(x => x.Count);
        int combinations = 1;
        foreach (var t in tmpd)
        {
          combinations *= t;
        }

        int couuntY = parametersList.Count;

        List<List<object>> combinationList = new List<List<object>>();
        for (int i = 0; i < combinations; i++)
        {
          List<object> list = new List<object>();
          for (int j = 0; j < parametersList.Count; j++)
          {
            list.Add(null);
          }
          combinationList.Add(list);
        }


        int tmp = combinations;
        for (int i = 0; i < parametersList.Count; i++)
        {
          tmp /= parametersList[i].Count;
          int o = 0;
          for (int j = 0; j < combinationList.Count;)
          {
            for (int k = 0; k < tmp; k++, j++)
            {

              combinationList[j][i] = parametersList[i][o];
            }
            o++;
            o %= parametersList[i].Count;
          }
        }
        foreach (var tpt in combinationList)
        {
          parameters = tpt.ToArray();

          try
          {
            met.Invoke(inst, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null);
          }
          catch (Exception e)
          {
            errorList.Add(new Error() { Exception = e, Type = type, MethodName = "RunMethods-DontEmptyValues", Parameters = parameters });
          }
        }
      }
    }

    private  void GetAndSetValue(object obj, PropertyInfo name)
    {
      object value = GetNonPublicIntPropertiesValue(obj, name.Name, obj.GetType());

      value = value ?? GetDefault(name.PropertyType, name.PropertyType);

      SetNonPublicIntPropertiesValue(obj, value, name.Name, obj.GetType());

      value = GetNonPublicIntPropertiesValue(obj, name.Name, obj.GetType());
      }

    private  IEnumerable<object> CreateInstaceOfType(Assembly assembly, Type type, Type baseType, bool tryNonEmpty =false)
    {
      var constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
      if (constructors != null)
      {
        foreach (var constract in constructors)
        {
          if (type.ContainsGenericParameters)
          {
            var genericArguments = type.GetGenericArguments();

            List<Type> genericType = new List<Type>();
            foreach (var genericArgument in genericArguments)
            {
              Type generic = genericArgument.BaseType;
              genericType.Add(generic);
            }

            Type constructed = type.MakeGenericType(genericType.ToArray());

            object[] parameters = null;
            List<object> parametersList = new List<object>();
            foreach (var param in constract.GetParameters())
            {
              Type t = param.ParameterType;
              if (t.IsGenericParameter)
              {
                parametersList.Add(GetDefault(t.BaseType, baseType,tryNonEmpty));
              }
              else
              {

                parametersList.Add(GetDefault(t, baseType, tryNonEmpty));
              }
            }

            parameters = parametersList.ToArray();

            object inst = null;

            try
            {
              inst = Activator.CreateInstance(constructed, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null, null);
            }
            catch (Exception e1)
            {
              errorList.Add(new Error() { Exception = e1, Type = type, Parameters = parameters, MethodName = "CreateInstaceOfType-ActivatorCreateInstance" });
            }

            yield return inst;
          }
          else
          {
            object[] parameters = null;
            List< List<object>> parametersList = new List<List<object>>();
            foreach (var param in constract.GetParameters())
            {
              Type t = param.ParameterType;
              List<object> item = tryNonEmpty?GetDefaults(t,baseType, tryNonEmpty).ToList(): new List<object>() { GetDefault(t, baseType, tryNonEmpty) };
              if(!item.Contains(null))
                item.Add(null);
              parametersList.Add(item);
            }

            var tmpd= parametersList.Select(x => x.Count);
            int combinations = 1;
            foreach (var t in tmpd)
            {
              combinations *= t;
            }

            int couuntY = parametersList.Count;

            List<List<object>> combinationList = new List<List<object>>();
            for (int i =0;i<combinations; i++)
            {
              List<object> list = new List<object>();
              for (int j = 0;j< parametersList.Count; j++)
              {
                list.Add(null);
              }
              combinationList.Add(list);
            }


            int tmp = combinations;
            for (int i=0; i< parametersList.Count; i++)
            {
              tmp /= parametersList[i].Count;
              int o = 0;
              for (int j = 0; j < combinationList.Count; )
              {
                for (int k = 0; k < tmp; k++, j++)
                {

                  combinationList[j][i] = parametersList[i][o];
                }
                o++;
                o %= parametersList[i].Count;
              }
            }

            foreach (var tpt in combinationList)
            {
              parameters = tpt.ToArray();

              object inst = null;

              try
              {
                inst = constract.Invoke(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null);
              }
              catch (Exception e)
              {
                errorList.Add(new Error() { Exception = e, Type = type, Parameters = parameters, MethodName = "CreateInstaceOfType-ConstructorInvoke" });

                try
                {
                  inst = assembly?.CreateInstance($"{type.FullName}", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null, null);
                }
                catch (Exception e1)
                {
                  errorList.Add(new Error() { Exception = e1, Type = type, Parameters = parameters, MethodName = "CreateInstaceOfType-AssemblyCreateInstance" });
                }
              }

              yield return inst;
            }

          }
        }
      }
    }

    public IEnumerable<object> GetDefaults(Type type, Type baseType, bool tryNonEmpty = false)
    {
      IEnumerable<Type> implementations = new List<Type>();
      if (type.IsInterface)
      {
        foreach (var assembly in assemblies)
        {
          IEnumerable<Type> nextAssemlbyImplementations = assembly.GetTypes().Where(x => x.GetInterfaces().Where(y => y.FullName.Equals(type.FullName)).Select(y => y).Any()).Select(y => y);
          if (implementations == null)
          {
            implementations = nextAssemlbyImplementations;
          }
          else
          {
            implementations = implementations.Union(nextAssemlbyImplementations);
          }
        }

        for (int i = 0; i < implementations.Count(); i++)
        {
          implementations = GetInheritClasses(implementations.ElementAt(i), implementations);
        }

        if (SearchImpelentationInSourceAssembly)
          try
          {
            if (!type.Namespace.StartsWith("System") || AllowSearchInMicrosoftAssembly)
            {
              var interfaceAssembly = Assembly.GetAssembly(type);
              var interfaceImplementations = interfaceAssembly.GetTypes().Where(x => x.GetInterfaces().Where(y => y.FullName.Equals(type.FullName)).Select(y => y).Any()).Select(y => y);
              implementations = implementations.Union(interfaceImplementations);
            }
          }
          catch (Exception e)
          {

          }
      }
      else
      if (type.IsAbstract)
      {

        foreach (var assembly in assemblies)
        {
          IEnumerable<Type> nextAssemlbyImplementations = assembly.GetTypes().Where(x => Type.Equals(x.BaseType, type) && !Type.Equals(x, baseType));
          if (implementations == null)
          {
            implementations = nextAssemlbyImplementations;
          }
          else
          {
            implementations = implementations.Union(nextAssemlbyImplementations);
          }
        }

        for (int i = 0; i < implementations.Count(); i++)
        {
          implementations = GetInheritClasses(implementations.ElementAt(i), implementations);
        }

        if (SearchImpelentationInSourceAssembly)
          try
          {
            if (!type.Namespace.StartsWith("System") || AllowSearchInMicrosoftAssembly)
            {
              var interfaceAssembly = Assembly.GetAssembly(type);
              var interfaceImplementations = interfaceAssembly.GetTypes().Where(x => Type.Equals(x.BaseType, type) && !Type.Equals(x, baseType));
              implementations = implementations.Union(interfaceImplementations);
            }
          }
          catch (Exception e)
          {

          }
      }
      else

      {
        implementations = GetInheritClasses(type, implementations, baseType);

        for (int i=0; i<implementations.Count(); i++)
        {
          implementations = GetInheritClasses(implementations.ElementAt(i), implementations, baseType);
        }


          if (SearchImpelentationInSourceAssembly)
          try
          {
            if (!type.Namespace.StartsWith("System") || AllowSearchInMicrosoftAssembly)
            {
              var interfaceAssembly = Assembly.GetAssembly(type);
              var interfaceImplementations = interfaceAssembly.GetTypes().Where(x => Type.Equals(x.BaseType, type) && !Type.Equals(x, baseType));
              implementations = implementations.Union(interfaceImplementations);
            }
          }
          catch (Exception e)
          {

          }
      }

      foreach (var typ in implementations)
        {
          yield return GetDefault(typ, baseType, tryNonEmpty);
        }
     
      if(!type.IsInterface && !type.IsAbstract )
        yield return GetDefault(type, baseType, tryNonEmpty);
      
    }

    private IEnumerable<Type> GetInheritClasses(Type type, IEnumerable<Type> implementations, Type baseType=null)
    {
      foreach (var assembly in assemblies)
      {
        IEnumerable<Type> nextAssemlbyImplementations = assembly.GetTypes().Where(x => Type.Equals(x.BaseType, type) && !Type.Equals(x, baseType));
        if (implementations == null)
        {
          implementations = nextAssemlbyImplementations;
        }
        else
        {
          implementations = implementations.Union(nextAssemlbyImplementations);
        }
      }

      return implementations;
    }

    public  object GetDefault(Type type, Type baseType, bool tryNonEmpty =false)
    {
      try
      {
        if (tryNonEmpty)
        {
          if (type.Equals(typeof(int)))
          {
            Random random = new Random();
            return random.Next();
          }
          if (type.Equals(typeof(double)))
          {
            Random random = new Random();
            return random.NextDouble();
          }
          if (type.Equals(typeof(float)))
          {
            Random random = new Random();
            return (float.Parse(random.NextDouble().ToString()));
          }
          if (type.Equals(typeof(decimal)))
          {
            Random random = new Random();
            return decimal.Parse(random.NextDouble().ToString());
          }
          if (type.Equals(typeof(string)))
          {
            return type.Name;
          }
          if (type.IsInterface)
          {
            IEnumerable<Type> tmp = null;
            foreach (var assembly in assemblies)
            {
              IEnumerable<Type> tmp2 = assembly.GetTypes().Where(x => x.GetInterfaces().Where(y => y.FullName.Equals(type.FullName)).Select(y => y).Any()).Select(y => y);
              if (tmp == null)
              {
                tmp = tmp2;
              }
              else
              {
                tmp = tmp.Union(tmp2);
              }
            }
            type = tmp.FirstOrDefault();
          }

        }
        return type.IsValueType ? Activator.CreateInstance(type) : CreateInstaceOfType(null, type, baseType).FirstOrDefault();
      }
      catch(Exception e)
      {
        errorList.Add(new Error() { Exception = e, Type = type,  MethodName = "GetDefault" });
      }
      return null;
    }




    public  void SetNonPublicIntFiledValue(object obj, object val, string fieldName, Type type)
    {
      try
      {
        FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

        field.SetValue(obj, val);
      }
      catch(Exception e)
      {
        errorList.Add(new Error() { Exception = e, Type = type, MethodName = "SetNonPublicIntFiledValue" });
      }
    }


    public  void SetNonPublicIntPropertiesValue(object obj, object val, string fieldName, Type type)
    {
      try { 
      var field = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

      field.SetValue(obj, val, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance,null,null,null);
      }
      catch (Exception e)
      {
        errorList.Add(new Error() { Exception = e, Type = type, MethodName = "SetNonPublicIntPropertiesValue" });
      }
    }

    public  object GetNonPublicIntFiledValue(object obj, string fieldName, Type type)
    {
      try{ 
      FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

      return field.GetValue(obj);
      }
      catch (Exception e)
      {
        errorList.Add(new Error() { Exception = e, Type = type, MethodName = "GetNonPublicIntFiledValue" });
      }
      return null;
    }


    public  object GetNonPublicIntPropertiesValue(object obj, string fieldName, Type type)
    {
      try { 
      var field = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

      return field.GetValue(obj, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance,null,null,null);
      }
      catch (Exception e)
      {
        errorList.Add(new Error() { Exception = e, Type = type, MethodName = "GetNonPublicIntPropertiesValue" });
      }
      return null;
    }

    public  object GetAndSetValue(object obj, FieldInfo name)
    {
      object value = GetNonPublicIntFiledValue(obj, name.Name, obj.GetType());

      value = value ?? GetDefault(name.FieldType, name.FieldType);

      SetNonPublicIntFiledValue(obj, value, name.Name, obj.GetType());

      value = GetNonPublicIntFiledValue(obj, name.Name, obj.GetType());

      return value;
    }

    public void SetMaxParalelism(int MaxParalelism)
    {
      _MaxParalelism = MaxParalelism;
    }
  }
}
