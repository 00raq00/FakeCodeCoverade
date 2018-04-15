
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
    ConcurrentBag<Error> errorList = new ConcurrentBag<Error>();
    public void RunCovererOnAssembly(string assemblyName)
    {
      Assembly assembly = Assembly.Load(assemblyName);

      foreach (var type in assembly.GetTypes())
      {
        foreach (var inst in CreateInstaceOfType(assembly, type).Union(CreateInstaceOfType(assembly, type,true)))
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
      object[] parameters = null;
      List<object> parametersList = new List<object>();
      foreach (var param in met.GetParameters())
      {
        Type paramType = param.ParameterType;
        parametersList.Add(GetDefault(paramType));
      }
      parameters = parametersList.ToArray();

      try
      {
        met.Invoke(inst, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null);
      }
      catch (Exception e)
      {
        errorList.Add(new Error() { Exception = e, Type = type, MethodName = "RunMethods-DefaultValues", Parameters=parameters });
      }

      parameters = null;
      parametersList = new List<object>();
      foreach (var param in met.GetParameters())
      {
        Type paramType = param.ParameterType;
        parametersList.Add(GetDefault(paramType, true));
      }
      parameters = parametersList.ToArray();
      try
      {
        met.Invoke(inst, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.CreateInstance, null, parameters, null);
      }
      catch (Exception e)
      {
        errorList.Add(new Error() { Exception = e, Type = type, MethodName = "RunMethods-DNonEmptyValues", Parameters = parameters });
      }
    }

    private  void GetAndSetValue(object obj, PropertyInfo name)
    {
      object value = GetNonPublicIntPropertiesValue(obj, name.Name, obj.GetType());

      value = value ?? GetDefault(name.PropertyType);

      SetNonPublicIntPropertiesValue(obj, value, name.Name, obj.GetType());

      value = GetNonPublicIntPropertiesValue(obj, name.Name, obj.GetType());
      }

    private  IEnumerable<object> CreateInstaceOfType(Assembly assembly, Type type, bool tryNonEmpty=false)
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
                parametersList.Add(GetDefault(t.BaseType, tryNonEmpty));
              }
              else
              {

                parametersList.Add(GetDefault(t, tryNonEmpty));
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
            List<object> parametersList = new List<object>();
            foreach (var param in constract.GetParameters())
            {
              Type t = param.ParameterType;
              parametersList.Add(GetDefault(t, tryNonEmpty));
            }

            parameters = parametersList.ToArray();

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

    public  object GetDefault(Type type, bool tryNonEmpty=false)
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
        }
        return type.IsValueType ? Activator.CreateInstance(type) : CreateInstaceOfType(null, type).FirstOrDefault();
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

      value = value ?? GetDefault(name.FieldType);

      SetNonPublicIntFiledValue(obj, value, name.Name, obj.GetType());

      value = GetNonPublicIntFiledValue(obj, name.Name, obj.GetType());

      return value;
    }

  }
}
