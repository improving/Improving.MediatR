namespace Improving.MediatR
{
    using System;
    using System.Collections.Concurrent;
    using Castle.DynamicProxy;

    [AttributeUsage(AttributeTargets.Class)]
    public class StopOnFailureAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<Type, bool> Cache
            = new ConcurrentDictionary<Type, bool>();

        public static bool IsDefined(object instance)
        {
            return Cache.GetOrAdd(ProxyUtil.GetUnproxiedType(instance),
                t => t.IsDefined(typeof(StopOnFailureAttribute), false));
        }
    }
}
