
using AutoCodeCoverage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoCodeCoverage
{
  public  class AutoCodeCoverer
  {
    ConcurrentBag<Error> errorList = new ConcurrentBag<Error>();
    ConcurrentDictionary<Type,object> injectionDictionary = new ConcurrentDictionary<Type, object>();
    
    public void SetInstanceToInject(Type type, object instance)
    {
      if(type.IsInstanceOfType(instance))
      {
        injectionDictionary[type] = instance;
      }
    }

    List<Assembly> assemblies=new List<Assembly>();

    private AutoCoverOptions autoCoverOptions;

    public AutoCodeCoverer():this(new AutoCoverOptions())
    {
      
    }

    public AutoCodeCoverer(AutoCoverOptions autoCoverOptions)
    {
      this.autoCoverOptions = autoCoverOptions;
    }

    public bool UseInstanceInjection
  {
    get
    {
      return injectionDictionary.Count > 0;
    }
  }
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
            errorList.Add(new Error() { Exception = e, Parameters = new object[] { assemblyName }, ErrorType = ErrorTypeEnum.LoadAssemblyByName });
          }
        }
        foreach (var assembly in assemblies)
        {
          foreach (var type in assembly.GetTypes().Where(x => !x.IsAbstract&& !x.IsInterface) )
          {
            string full = type.FullName;
            var enumerable = CreateInstaceOfType(assembly, type, type).ToList();
            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = autoCoverOptions.MaxDegreeOfParallelismForCreateInstances };

            Parallel.ForEach(enumerable, parallelOptions, inst =>
            {
              ProcessInstance(inst, type);
            });

            enumerable = CreateInstaceOfType(assembly, type, type, true).ToList();
            Parallel.ForEach(enumerable, parallelOptions, inst =>
            {
              ProcessInstance(inst, type);
            });
          }
        }
      }
    }

    private void ProcessInstance(object inst, Type type)
    {
      if (inst != null)
      {
        var members = inst.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
        
        if (autoCoverOptions.InvokeMethods)
        {
          var methods = inst.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
          if (!autoCoverOptions.TryCoverBaseExternal)
          {
            methods = methods.Where(x => (assemblies.Exists(y => y.Equals(x.DeclaringType.Assembly)))).ToArray();
          }
          ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = autoCoverOptions.MaxDegreeOfParallelismForMethodsInvokes };

          Parallel.ForEach(methods, parallelOptions, met =>
          {
            RunMethods(type, inst, met);
          });
        }

        if (autoCoverOptions.CoverFields)
        {
          var fields = inst.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

          if(!autoCoverOptions.TryCoverBaseExternal)
          {
            fields = fields.Where(x => (assemblies.Exists(y=>y.Equals(x.DeclaringType.Assembly)))).ToArray();
          }

          foreach (var field in fields)
          {
            GetAndSetValue(inst, field);
          }
        }

        if (autoCoverOptions.CoverProperties)
        {
          var properties = inst.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

          if (!autoCoverOptions.TryCoverBaseExternal)
          {
            properties = properties.Where(x => (assemblies.Exists(y => y.Equals(x.DeclaringType.Assembly)))).ToArray();
          }
          foreach (var property in properties)
          {
            GetAndSetValue(inst, property);
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
        bool skipDefualtInvoke = false;
        object[] parameters = null;
        List<object> parametersList = new List<object>();
        foreach (var param in met.GetParameters())
        {
          Type paramType = param.ParameterType;
          parametersList.Add(GetDefault(paramType, paramType,false, type).FirstOrDefault());
        }
        parameters = parametersList.ToArray();

        if (!autoCoverOptions.AllowNullsAsMethodParameter)
        {
          if (parameters.Where(x => x == null).Any())
            skipDefualtInvoke = true;
        }

        if (!skipDefualtInvoke)
        {
          try
          {
            met.Invoke(inst, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null);
          }
          catch (Exception e)
          {
            errorList.Add(new Error() { Exception = e, Type = type, ErrorType = ErrorTypeEnum.InvokeMethod, Parameters = parameters });
          }
        }
      }
      {
        object[] parameters = null;
        List<List<object>> parametersList = new List<List<object>>();
        foreach (var param in met.GetParameters())
        {
          Type t = param.ParameterType;
          List<object> item = GetDefaults(t, type, true, new[] { type, t }).ToList();
          if (!item.Contains(null)&&!injectionDictionary.ContainsKey(t))
            item.Add(null);
          parametersList.Add(item);
        }

        if (!autoCoverOptions.AllowNullsAsMethodParameter)
        {
          foreach (var par in parametersList)
          {
            for (int i = par.Count - 1; i >= 0; i--)
            {
              if (par[i] == null)
              {
                par.RemoveAt(i);
              }
            }
          }
        }

        List<List<object>> combinationList = CreateParameterCombinations(parametersList, autoCoverOptions.TopParameterCombinationsForInvokeMethods);


        ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = autoCoverOptions.MaxDegreeOfParallelismForCombinationOfParametersMethodInvokes };

        Parallel.ForEach(combinationList, parallelOptions, tpt =>
        {
          parameters = tpt.ToArray();

          try
          {
            met.Invoke(inst, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null);
          }
          catch (Exception e)
          {
            errorList.Add(new Error() { Exception = e, Type = type, ErrorType = ErrorTypeEnum.InvokeMethod, Parameters = parameters });
          }
        });
      }
    }

    private  List<List<object>> CreateParameterCombinations(List<List<object>> parametersList, int topParameterCombinations)
    {
      for(int i =parametersList.Count-1; i>=0;i--)
      {
        parametersList[i] = parametersList[i].Distinct().ToList();
      }


      var tmpd = parametersList.Select(x => x.Count);
      int combinations = 1;
      foreach (var t in tmpd)
      {
        combinations *= t;
      }

      int couuntY = parametersList.Count;

      List <List<object>> combinationList = new List<List<object>>();
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

      int count = combinations > topParameterCombinations ? topParameterCombinations : combinations;
      if(autoCoverOptions.AllowRandomizeParametersWithTopCount)
      {
        if(count!= combinations)
        {
          Random r = new Random(combinations);

          List<int> randomizeItem = new List<int>();
          List<List<object>> randomizedCombinationList = new List<List<object>>();

          while(randomizedCombinationList.Count< topParameterCombinations)
          {
            int rand = r.Next()% combinations;
            if (randomizeItem.Contains(rand))
            {
              continue;
            }
            randomizeItem.Add(rand);
            randomizedCombinationList.Add(combinationList[rand]);
          }

          return randomizedCombinationList;

        }
      }

      return combinationList.Take(count).ToList();
    }

    private  void GetAndSetValue(object obj, PropertyInfo name)
    {
      object value = GetNonPublicIntPropertiesValue(obj, name.Name, obj.GetType());

      value = value ?? GetDefault(name.PropertyType, name.PropertyType).FirstOrDefault();

      SetNonPublicIntPropertiesValue(obj, value, name.Name, obj.GetType());

      value = GetNonPublicIntPropertiesValue(obj, name.Name, obj.GetType());
      }

    private IEnumerable<object> CreateInstaceOfType(Assembly assembly, Type type, Type baseType, bool tryNonEmpty = false, params Type[] createForType)
    {
      if(UseInstanceInjection)
      {
        if(injectionDictionary.Keys.Contains(type))
        {
          yield return injectionDictionary[type];
          yield break;
        }
      }
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
            List<List<object>> parametersList = new List<List<object>>();
            foreach (var param in constract.GetParameters())
            {
              Type t = param.ParameterType;
              if (t.IsGenericParameter)
              {
                parametersList.Add(GetDefault(t.BaseType, baseType, tryNonEmpty, createForType).ToList());
              }
              else
              {

                List<object> item1 = GetDefault(t, baseType, tryNonEmpty, createForType).ToList();
                //var list = GetDefault(t, item1);

                parametersList.Add(item1);
              }
            }

            List<List<object>> combinationList = CreateParameterCombinations(parametersList, autoCoverOptions.TopParameterCombinationsForCreateInstances);

            foreach (var tpt in combinationList)
            {
              parameters = tpt.ToArray();


              if (!autoCoverOptions.AllowNullsAsConstractorParameter)
              {
                if (parameters.Where(x => x == null).Any())
                  continue;
              }
              object inst = null;

              try
              {
                inst = constract.Invoke(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null);
              }
              catch (Exception e)
              {
                try
                {
                  inst = Activator.CreateInstance(constructed, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null, null);
                }
                catch (Exception e1)
                {
                  errorList.Add(new Error() { Exception = e1, Type = type, Parameters = parameters, ErrorType = ErrorTypeEnum.CreateInstance });
                }
              }

              yield return inst;
            }
          }
          else
          {
            object[] parameters = null;
            List<List<object>> parametersList = new List<List<object>>();
            foreach (var param in constract.GetParameters())
            {
              var types = createForType;

              Type t = param.ParameterType;
              var listLtype = new List<Type>();
              if (types != null)
              {
                listLtype.AddRange(types);
              }
              listLtype.Add(baseType);
              listLtype.Add(type);
              listLtype.Add(t);

              List<object> item = tryNonEmpty ? GetDefaults(t, baseType, tryNonEmpty, listLtype.Distinct().ToArray()).ToList() : new List<object>() { GetDefault(t, baseType, tryNonEmpty, createForType).FirstOrDefault() };
              if (!item.Contains(null) && !injectionDictionary.ContainsKey(t))
                item.Add(null);
              parametersList.Add(item);
            }

            if (!autoCoverOptions.AllowNullsAsConstractorParameter)
            {
              foreach (var par in parametersList)
              {
                for (int i = par.Count - 1; i >= 0; i--)
                {
                  if (par[i] == null)
                  {
                    par.RemoveAt(i);
                  }
                }
              }
            }
            bool skipInvokeConstructor = false;
            if (parametersList.Where(x => x.Count == 0).Any())
            {
              skipInvokeConstructor = true;
            }
            if (!skipInvokeConstructor)
            {
              List<List<object>> combinationList = CreateParameterCombinations(parametersList, autoCoverOptions.TopParameterCombinationsForCreateInstances);

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
                  try
                  {
                    inst = assembly?.CreateInstance($"{type.FullName}", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null, null);
                  }
                  catch (Exception e1)
                  {
                    errorList.Add(new Error() { Exception = e1, Type = type, Parameters = parameters, ErrorType = ErrorTypeEnum.CreateInstance });
                  }
                }

                yield return inst;
              }
            }
          }
        }
      }
    }

    private object GetDefault(Type t, IEnumerable<object> obj)
    {
      return GetType().GetMethod("GetDefaultList").MakeGenericMethod(t).Invoke(this, new[] { obj });
    }

    public List<T> GetDefaultList<T>(IEnumerable<object> obj)
    {
      List<T> list = new List<T>();
      foreach(var ob in obj)
      {
        list.Add((T)ob);
      }
      return list;
    }

    private IEnumerable<object> GetDefaults(Type type, Type baseType, bool tryNonEmpty = false, params Type[] createForType)
    {
      if (UseInstanceInjection)
      {
        if (injectionDictionary.Keys.Contains(type))
        {
          yield return injectionDictionary[type];
          yield break;
        }
      }
      IEnumerable<Type> implementations = new List<Type>();
      if (type.IsInterface)
      {
        foreach (var assembly in assemblies)
        {
          IEnumerable<Type> nextAssemlbyImplementations = FindInterfaceImplementations(type, assembly);

          implementations = implementations.Union(nextAssemlbyImplementations);
        }

        for (int i = 0; i < implementations.Count(); i++)
        {
          implementations = GetInheritingClassesFromAllAssemblies(implementations.ElementAt(i), implementations);
        }

        if (autoCoverOptions.SearchImplentationInSourceAssembly)
          try
          {
            if (!type.Namespace.StartsWith("System") || autoCoverOptions.AllowSearchInMicrosoftAssembly)
            {
              var interfaceAssembly = Assembly.GetAssembly(type);
              IEnumerable<Type> interfaceImplementations = FindInterfaceImplementations(type, interfaceAssembly);
              implementations = implementations.Union(interfaceImplementations);
            }
          }
          catch (Exception e)
          {
            errorList.Add(new Error() { Exception = e, Parameters = new object[] { type }, ErrorType = ErrorTypeEnum.SearchImplementationInSourceAssembly });
          }
      }
      else
      if (type.IsClass)
      {

        implementations = GetInheritingClassesFromAllAssemblies(type, implementations, baseType, createForType);

        for (int i = 0; i < implementations.Count(); i++)
        {
          implementations = GetInheritingClassesFromAllAssemblies(implementations.ElementAt(i), implementations, baseType,createForType);
        }

        if (autoCoverOptions.SearchImplentationInSourceAssembly)
          try
          {
            if (!type.Namespace.StartsWith("System") || autoCoverOptions.AllowSearchInMicrosoftAssembly)
            {
              var baseAssembly = Assembly.GetAssembly(type);
              var inhiritClasses = FindInheritingClasses(type, baseType, baseAssembly, createForType);
              implementations = implementations.Union(inhiritClasses);
            }
          }
          catch (Exception e)
          {
            errorList.Add(new Error() { Exception = e, Parameters = new object[] { type }, ErrorType = ErrorTypeEnum.SearchImplementationInSourceAssembly });
          }
      }

      foreach (var typ in implementations)
        {
        List<object> list = GetDefault(typ, baseType, tryNonEmpty, createForType).ToList();
        foreach(var obj in list)
        {
                    yield return obj;
        }
        }
     
      if(!type.IsInterface && !type.IsAbstract )
      {
        List<object> list1 = GetDefault(type, baseType, tryNonEmpty, createForType).ToList();
        foreach (var obj in list1)
        {
          yield return obj;
        }
      }
    
    }

    private static IEnumerable<Type> FindInheritingClasses(Type type, Type baseType, Assembly assembly, params Type[] createForType)
    {
      List<Type> list = new List<Type>();
      IEnumerable<Type> enumerable = assembly.GetTypes().Where(x => Type.Equals(x.BaseType, type) && !Type.Equals(x, baseType));
      foreach (var t in enumerable)
      {
        bool exist = false;
        foreach(var cft in createForType)
        {
          if (Type.Equals(t, cft))
          {
            exist = true;
            break;
          }
        }
        if (!exist)
        {
          list.Add(t);
        }
      }

      return list;
    }

    private static IEnumerable<Type> FindInterfaceImplementations(Type type, Assembly interfaceAssembly)
    {
      return interfaceAssembly.GetTypes().Where(x => x.GetInterfaces().Where(y => y.Equals(type)).Select(y => y).Any()).Select(y => y);
    }

    private IEnumerable<Type> GetInheritingClassesFromAllAssemblies(Type type, IEnumerable<Type> implementations, Type baseType = null, params Type[] createForType)
    {
      foreach (var assembly in assemblies)
      {
        IEnumerable<Type> nextAssemlbyImplementations = FindInheritingClasses(type, baseType, assembly, createForType);

          implementations = implementations.Union(nextAssemlbyImplementations);
      }

      return implementations;
    }

    private IEnumerable<object> GetDefault(Type type, Type baseType, bool tryNonEmpty = false, params Type[] createForType)
    {
      if (UseInstanceInjection)
      {
        if (injectionDictionary.Keys.Contains(type))
        {
          yield return injectionDictionary[type];
          yield break;

        }
      }

      if (tryNonEmpty)
      {
        if (type.Equals(typeof(int)))
        {
          Random random = new Random();
          yield return (int)random.Next();

          yield break;
        }
        if (type.Equals(typeof(double)))
        {
          Random random = new Random();
          yield return random.NextDouble();

          yield break;
        }
        if (type.Equals(typeof(float)))
        {
          Random random = new Random();
          yield return (float.Parse(random.NextDouble().ToString()));

          yield break;
        }
        if (type.Equals(typeof(decimal)))
        {
          Random random = new Random();
          yield return decimal.Parse(random.NextDouble().ToString());

          yield break;
        }
        if (type.Equals(typeof(string)))
        {
          yield return (string)type.Name;

          yield break;
        }

        List<object> obj = new List<object>();
        try
        {
          if (type.IsInterface)
          {
            IEnumerable<Type> tmp = null;
            foreach (var assembly in assemblies)
            {
              IEnumerable<Type> tmp2 = assembly.GetTypes().Where(x => x.GetInterfaces().Where(y => y.Equals(type)).Select(y => y).Any()).Select(y => y);
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

          if (type.IsValueType)
          {
            obj.Add(Activator.CreateInstance(type));
          }
          else
          {
            if (autoCoverOptions.TopCountOfSameObjectInstances == 1)
            {
              obj.Add(CreateInstaceOfType(null, type, baseType, tryNonEmpty, createForType).FirstOrDefault());
            }
            else
            {
              obj.AddRange(CreateInstaceOfType(null, type, baseType, tryNonEmpty, createForType).Take(autoCoverOptions.TopCountOfSameObjectInstances));
            }
          }

        }
        catch (Exception e)
        {
          errorList.Add(new Error() { Exception = e, Parameters = new object[] { type }, ErrorType = ErrorTypeEnum.CreateDefaultValue });
        }
        foreach (var ob in obj)
        {
          yield return ob;
        }
      }
      else
      {
        yield return null;
      }
    }




    private void SetNonPublicIntFiledValue(object obj, object val, string fieldName, Type type)
    {
      try
      {
        FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

        field.SetValue(obj, val);
      }
      catch(Exception e)
      {
        errorList.Add(new Error() { Exception = e, Parameters = new object[] { type,fieldName }, ErrorType = ErrorTypeEnum.SetFieldValue });
      }
    }


    private void SetNonPublicIntPropertiesValue(object obj, object val, string propertyName, Type type)
    {
      try
      {
        var field = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);
        if (field.GetSetMethod() != null)
        {
          field.SetValue(obj, val, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null);
        }
      }
      catch (Exception e)
      {
        errorList.Add(new Error() { Exception = e, Parameters = new object[] { type, propertyName }, ErrorType = ErrorTypeEnum.SetPropertyValue });
      }
    }

    private object GetNonPublicIntFiledValue(object obj, string fieldName, Type type)
    {
      try{ 
      FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

      return field.GetValue(obj);
      }
      catch (Exception e)
      {
        errorList.Add(new Error() { Exception = e, Parameters = new object[] { type, fieldName }, ErrorType = ErrorTypeEnum.GetFieldValue });
      }
      return null;
    }


    private object GetNonPublicIntPropertiesValue(object obj, string fieldName, Type type)
    {
      try
      {
        var field = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance);

        return field.GetValue(obj, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null);
      }
      catch (Exception e)
      {
        errorList.Add(new Error() { Exception = e, Parameters = new object[] { type, fieldName }, ErrorType = ErrorTypeEnum.GetPropertyValue });
      }
      return null;
    }

    private object GetAndSetValue(object obj, FieldInfo name)
    {
      object value = GetNonPublicIntFiledValue(obj, name.Name, obj.GetType());

      value = value ?? GetDefault(name.FieldType, name.FieldType).FirstOrDefault();

      SetNonPublicIntFiledValue(obj, value, name.Name, obj.GetType());

      value = GetNonPublicIntFiledValue(obj, name.Name, obj.GetType());

      return value;
    }
     
  }
}
