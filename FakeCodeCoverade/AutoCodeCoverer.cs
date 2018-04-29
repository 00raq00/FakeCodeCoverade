
using AutoCodeCoverade;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoCodeCoverade
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
          ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = autoCoverOptions.MaxDegreeOfParallelismForCombinationOfParametersMethodInvokes };

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
          errorList.Add(new Error() { Exception = e, Type = type, ErrorType = ErrorTypeEnum.InvokeMethod, Parameters = parameters });
        }
      }
      {
        object[] parameters = null;
        List<List<object>> parametersList = new List<List<object>>();
        foreach (var param in met.GetParameters())
        {
          Type t = param.ParameterType;
          List<object> item = GetDefaults(t, type, true).ToList();
          if (!injectionDictionary.ContainsKey(t))
            item.Add(null);
          parametersList.Add(item);
        }
        List<List<object>> combinationList = CreateParameterCombinations(parametersList);


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

    private static List<List<object>> CreateParameterCombinations(List<List<object>> parametersList)
    {
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

      return combinationList;
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
              errorList.Add(new Error() { Exception = e1, Type = type, Parameters = parameters, ErrorType=ErrorTypeEnum.CreateInstance });
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
              if(!item.Contains(null) && !injectionDictionary.ContainsKey(t))
                item.Add(null);
              parametersList.Add(item);
            }

          
            
            List<List<object>> combinationList = CreateParameterCombinations(parametersList);

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
                  errorList.Add(new Error() { Exception = e1, Type = type, Parameters = parameters, ErrorType=ErrorTypeEnum.CreateInstance });
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

        if (autoCoverOptions.SearchImpelentationInSourceAssembly)
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

        implementations = GetInheritingClassesFromAllAssemblies(type, implementations, baseType);

        for (int i = 0; i < implementations.Count(); i++)
        {
          implementations = GetInheritingClassesFromAllAssemblies(implementations.ElementAt(i), implementations);
        }

        if (autoCoverOptions.SearchImpelentationInSourceAssembly)
          try
          {
            if (!type.Namespace.StartsWith("System") || autoCoverOptions.AllowSearchInMicrosoftAssembly)
            {
              var baseAssembly = Assembly.GetAssembly(type);
              var inhiritClasses = FindInheritingClasses(type, baseType, baseAssembly);
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
          yield return GetDefault(typ, baseType, tryNonEmpty);
        }
     
      if(!type.IsInterface && !type.IsAbstract )
        yield return GetDefault(type, baseType, tryNonEmpty);
      
    }

    private static IEnumerable<Type> FindInheritingClasses(Type type, Type baseType, Assembly assembly)
    {
      return assembly.GetTypes().Where(x => Type.Equals(x.BaseType, type) && !Type.Equals(x, baseType));
    }

    private static IEnumerable<Type> FindInterfaceImplementations(Type type, Assembly interfaceAssembly)
    {
      return interfaceAssembly.GetTypes().Where(x => x.GetInterfaces().Where(y => y.Equals(type)).Select(y => y).Any()).Select(y => y);
    }

    private IEnumerable<Type> GetInheritingClassesFromAllAssemblies(Type type, IEnumerable<Type> implementations, Type baseType=null)
    {
      foreach (var assembly in assemblies)
      {
        IEnumerable<Type> nextAssemlbyImplementations = FindInheritingClasses(type, baseType, assembly);

          implementations = implementations.Union(nextAssemlbyImplementations);
      }

      return implementations;
    }

    public  object GetDefault(Type type, Type baseType, bool tryNonEmpty =false)
    {
      if (UseInstanceInjection)
      {
        if (injectionDictionary.Keys.Contains(type))
        {
           return injectionDictionary[type];
          
        }
      }
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

        }
        return type.IsValueType ? Activator.CreateInstance(type) : CreateInstaceOfType(null, type, baseType).FirstOrDefault();
      }
      catch(Exception e)
      {
        errorList.Add(new Error() { Exception = e, Parameters = new object[] { type }, ErrorType = ErrorTypeEnum.CreateDefaultValue });
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
        errorList.Add(new Error() { Exception = e, Parameters = new object[] { type,fieldName }, ErrorType = ErrorTypeEnum.SetFieldValue });
      }
    }


    public void SetNonPublicIntPropertiesValue(object obj, object val, string propertyName, Type type)
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

    public  object GetNonPublicIntFiledValue(object obj, string fieldName, Type type)
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


    public object GetNonPublicIntPropertiesValue(object obj, string fieldName, Type type)
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

    public  object GetAndSetValue(object obj, FieldInfo name)
    {
      object value = GetNonPublicIntFiledValue(obj, name.Name, obj.GetType());

      value = value ?? GetDefault(name.FieldType, name.FieldType);

      SetNonPublicIntFiledValue(obj, value, name.Name, obj.GetType());

      value = GetNonPublicIntFiledValue(obj, name.Name, obj.GetType());

      return value;
    }
     
  }
}
