namespace Improving.MediatR
{
    using System;
    using System.Collections.Concurrent;
    using Castle.DynamicProxy;

    [AttributeUsage(AttributeTargets.Class)]
    public class RelativeOrderAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<Type, RelativeOrderAttribute> Cache
            = new ConcurrentDictionary<Type, RelativeOrderAttribute>();

        public RelativeOrderAttribute()
            : this(Int32.MaxValue)
        {
        }

        public RelativeOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; set; }

        public static RelativeOrderAttribute Get(Type type)
        {
            var attributes = type.GetCustomAttributes(typeof (RelativeOrderAttribute), false);
            return attributes.Length == 1 ? (RelativeOrderAttribute)attributes[0] : null;
        }

        public static int Compare(object item1, object item2)
        {
            int order1, order2;
            var attrib2 = Cache.GetOrAdd(ProxyUtil.GetUnproxiedType(item2), Get);
            if (attrib2 != null)
                order2 = attrib2.Order;
            else
                return -1;

            var attrib1 = Cache.GetOrAdd(ProxyUtil.GetUnproxiedType(item1), Get);
            if (attrib1 != null)
                order1 = attrib1.Order;
            else
                return 1;

            return order1 - order2;
        }
    }
}