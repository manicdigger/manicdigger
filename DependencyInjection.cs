using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DependencyInjection
{
    public class MyLinq
    {
        public static IEnumerable Reverse(Array a)
        {
            for (int i = a.Length - 1; i >= 0; i--)
            {
                yield return a.GetValue(i);
            }
        }
    }
    public interface IModule
    {
        void Load(IBinder binder);
    }
    public class Modules
    {
        public Modules(params IModule[] modules)
        {
            this.modules = modules;
        }
        IModule[] modules;
        public void Load(IBinder k)
        {
            foreach (var m in modules)
            {
                m.Load(k);
            }
        }
    }
    public interface IKernel
    {
        T Get<T>();
    }
    public interface IBinder
    {
        void Bind<A, B>() where B : A;
        void Bind<A, B>(Behavior behavior) where B : A;
    }
    public enum Behavior
    {
        SingletonBehavior,
        TransientBehavior,
    }
    public interface IFactory<T>
    {
        T Create();
    }
    class GoodFactory<A> : IFactory<A>
    {
        public GoodFactory(IKernelInside kernel)
        {
            this.kernel = kernel;
        }
        IKernelInside kernel;
        #region IFactory<A> Members
        public A Create()
        {
            return (A)kernel.Get(typeof(A), Behavior.TransientBehavior);
        }
        #endregion
    }
    internal interface IKernelInside
    {
        object Get(Type tt, Behavior? override_behavior);
    }
    public class KernelAndBinder : IKernel, IBinder
    {
        KernelAndBinderInside inside = new KernelAndBinderInside();
        #region IKernel Members
        public T Get<T>()
        {
            inside.calling_assembly = System.Reflection.Assembly.GetCallingAssembly();
            return inside.Get<T>();
        }
        #endregion
        #region IBinder Members
        public void Bind<A, B>() where B : A
        {
            inside.Bind<A, B>();
        }
        public void Bind<A, B>(Behavior behavior) where B : A
        {
            inside.Bind<A, B>(behavior);
        }
        public void BindInstance<T>(T o)
        {
            inside.BindInstance<T>(o);
        }
        #endregion
    }
    internal class KernelAndBinderInside : IKernel, IBinder, IKernelInside
    {
        public System.Reflection.Assembly calling_assembly;
        public T Get<T>()
        {
            return (T)Get(typeof(T), null);
        }
        public object Get(Type tt, Behavior? override_behavior)
        {
            var t = GetConcreteType(tt);
            var behavior = t.behavior;
            if (override_behavior != null)
            {
                behavior = override_behavior.Value;
            }
            if (t.b.IsGenericType)//wewnętrzne
            {
                if (t.b.GetGenericTypeDefinition() == typeof(GoodFactory<>))
                {
                    return t.b.GetConstructor(new Type[] { typeof(IKernelInside) }).Invoke(new IKernelInside[] { this });
                }
            }
            if (behavior == Behavior.SingletonBehavior)
            {
                if (all_objects.ContainsKey(t.b))
                {
                    return all_objects[t.b];
                }
            }
            var constructors = t.b.GetConstructors();
            object ready_object = null;
            foreach (var c in constructors)
            {
                var p = c.GetParameters();
                //if (!p.Any())
                if (p.Length == 0)
                {
                    //just create p - this is a parameterless constructor.
                }
                List<object> prepared_parameters = new List<object>();
                foreach (var pp in p)
                {
                    prepared_parameters.Add(Get(pp.ParameterType, null));
                }
                try
                {
                    ready_object = c.Invoke(prepared_parameters.ToArray());
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("constructor of {0} has thrown an exception", c.ToString()), e);
                }
                if (t.behavior == Behavior.SingletonBehavior)
                {
                    all_objects[t.b] = ready_object;
                }
            }
            var properties = t.b.GetProperties();
            foreach (var p in properties)
            {
                var attributes = p.GetCustomAttributes(true);
                foreach (var a in attributes)
                {
                    if (a is InjectAttribute)
                    {
                        //must inject to this property
                        p.SetValue(ready_object, Get(p.PropertyType, null), null);
                    }
                }
            }
            return ready_object;
        }
        Dictionary<Type, object> all_objects = new Dictionary<Type, object>();
        private BindEntry GetConcreteType(Type interface_type)
        {
            if (!bind_entries.ContainsKey(interface_type))
            {
                //try bind to any random class that implements needed interface type.
                var random_implementing = GetRandomImplementing(interface_type);
                if (random_implementing != null)
                {
                    return new BindEntry(random_implementing, Behavior.SingletonBehavior);
                }
                //try auto factory
                if (interface_type.IsGenericType)
                {
                    if (interface_type.GetGenericTypeDefinition() == typeof(IFactory<>))
                    {
                        var random_implementing2 = GetRandomImplementing(interface_type.GetGenericArguments()[0]);
                        var factory = typeof(GoodFactory<>).MakeGenericType(interface_type.GetGenericArguments()[0]);
                        return new BindEntry(factory, Behavior.SingletonBehavior);
                    }
                }
                //try self-bindable type
                if (interface_type.IsInterface)
                {
                    throw new Exception(string.Format("can't self-bind an interface ({0})", interface_type.Name));
                }
                //self-bind
                return new BindEntry(interface_type, Behavior.SingletonBehavior);
            }
            var t = bind_entries[interface_type];
            if (t.b.IsInterface)
            {
                //try redirect
                //t = GetConcreteType(t);
                throw new Exception();
            }
            return t;
        }
        private Type GetRandomImplementing(Type interface_type)
        {
            if (interface_type.IsInterface)
            {
                var types = AllPossibleTypes();
                //var types = calling_assembly.GetTypes();
                foreach (var type in types)
                {
                    //mylinq
                    //if (type.GetInterfaces().Any(v => v == interface_type)
                    //    && (!type.IsInterface))
                    if (AnyEqual(type.GetInterfaces(), interface_type)
                          && (!type.IsInterface))
                    {
                        //System.Diagnostics.Debug.Print
                        //System.Diagnostics.Debug.Flush();
                        Console.WriteLine(string.Format("warning - binding to a random class {0} that implements {1}, with singleton behavior.", type.ToString(), interface_type.ToString()));
                        return type;
                    }
                }
            }
            return null;
        }
        bool AnyEqual(Type[] types, Type v)
        {
            foreach (var t in types)
            {
                if (t == v)
                {
                    return true;
                }
            }
            return false;
        }
        private IEnumerable<Type> AllPossibleTypes()
        {
            var assemblies = MyLinq.Reverse(AppDomain.CurrentDomain.GetAssemblies());
            //calling assembly first
            foreach (var type in calling_assembly.GetTypes())
            {
                yield return type;
            }
            foreach (var assembly in assemblies)
            {
                foreach (var type in ((Assembly)assembly).GetTypes())
                {
                    yield return type;
                }
            }
        }
        public void Bind<A, B>() where B : A
        {
            bind_entries[typeof(A)] = new BindEntry(typeof(B), Behavior.SingletonBehavior);
        }
        public void Bind<A, B>(Behavior behavior) where B : A
        {
            bind_entries[typeof(A)] = new BindEntry(typeof(B), behavior);
        }
        Dictionary<Type, BindEntry> bind_entries = new Dictionary<Type, BindEntry>();
        struct BindEntry
        {
            public BindEntry(Type b, Behavior behavior)
            {
                this.b = b;
                this.behavior = behavior;
            }
            public Type b;
            public Behavior behavior;
        }
        void Switch<A, B>() where B : A
        {
            //for each object in all properties with [inject] A a, set B there.
        }
        public void BindInstance<T>(T o)
        {
            bind_entries[typeof(T)] = new BindEntry(o.GetType(), Behavior.SingletonBehavior);
            all_objects[typeof(T)] = o;
            all_objects[o.GetType()] = o;
        }
    }
    public class InjectAttribute : Attribute
    {
    }
}