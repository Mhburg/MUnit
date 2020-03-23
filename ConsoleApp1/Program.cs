using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

public static class OutInSpace
{
    public static void GetNamespace()
    {
        string name = typeof(OutInSpace).Namespace;

        // This condition evaluates to true.
        if (name == null)
        {
            Console.WriteLine("name is null");
        }
        else if (name == string.Empty)
        {
            Console.WriteLine("name is empty");
        }
        Console.WriteLine(typeof(OutInSpace).Namespace);
        Console.WriteLine(typeof(OutInSpace).FullName);
        Console.WriteLine(typeof(OutInSpace).GetMethod(nameof(OutInSpace.GetNamespace), BindingFlags.Public).GetType().ToString());
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class MockAttribute : Attribute
{

}

public class BaseClass
{

    public virtual void TestMethod() { }

    public virtual void OneGenericMethod<T>(T data)
    {
        switch (data)
        {
            case int num:
                Console.WriteLine("data is int.");
                break;
            default:
                throw new NotSupportedException();
        }
    }
}

public class DerivedClass : BaseClass
{
    public override void TestMethod()
    {
        base.TestMethod();
    }

    public void GetAttribute()
    {
        if (GetType().GetMethod("TestMethod").GetCustomAttributes(false).OfType<Attribute>().Any(attr => attr is MockAttribute))
        {
            Console.WriteLine("Method inherits attribute from base class.");
        }
        else
        {
            Console.WriteLine("Method can't find attribute.");
        }
    }

    
    public void GetReturnType()
    {
        if (GetType().GetMethod("TestMethod").ReturnType == null)
        {
            Console.WriteLine("Return type is null");
        }
        else
        {
            Console.WriteLine("Return type is " + GetType().GetMethod("TestMethod").ReturnType);

        }
    }
}




namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Thread.Sleep(1000);
            Thread.SpinWait(100000000);
            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0}", stopwatch.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
