namespace Improving.MediatR.Environment
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting.Messaging;
    using Castle.Core.Internal;

    public class EnvironmentScope : IDisposable
    {
        private static readonly 
            ConditionalWeakTable<InstanceIdentifier, EnvironmentScope> AppDomainInstances = 
                new ConditionalWeakTable<InstanceIdentifier, EnvironmentScope>();

        private static readonly string AmbientScopeKey = "MediatorEnv_" + Guid.NewGuid();

        private readonly InstanceIdentifier _instanceIdentifier;
        private readonly EnvironmentScope _parentScope;
        private readonly List<object> _items;
        private readonly IDictionary<string, object> _keyValues;
        private bool _disposed;

        public EnvironmentScope()
        {
            _items              = new List<object>();
            _keyValues          = new Dictionary<string, object>();
            _instanceIdentifier = new InstanceIdentifier();
             _parentScope       = GetAmbientScope();
            SetAmbientScope(this);
        }

        internal bool Implied { get; set; }

        #region Items

        public TItem Get<TItem>() where TItem : class
        {
            return Get(typeof (TItem)) as TItem;
        }

        public object Get(Type itemType)
        {
            object item;
            var elementType = itemType.GetCompatibleArrayItemType();
            if (elementType != null)
            {
                item = GetArray(itemType, elementType);
            }
            else if (itemType.IsGenericType &&
                     ListTypes.Contains(itemType.GetGenericTypeDefinition()))
            {
                item = GetList(itemType);
            }
            else
            {
                item = _items.Where(itemType.IsInstanceOfType).FirstOrDefault();
                if (item == null && _parentScope != null)
                    item = _parentScope.Get(itemType);
            }
            return item;
        }

        public bool Contains(Type itemType)
        {
            var elementType = itemType.GetCompatibleArrayItemType();
            if (elementType != null)
                itemType = elementType;
            else if (itemType.IsGenericType &&
                     ListTypes.Contains(itemType.GetGenericTypeDefinition()))
                itemType = itemType.GetGenericArguments()[0];
            return _items.Any(itemType.IsInstanceOfType) ||
                    ((_parentScope != null) && _parentScope.Contains(itemType));
        }

        public bool Contains<TItem>() where TItem : class
        {
            return Contains(typeof(TItem));
        }

        public EnvironmentScope Add(params object[] items)
        {
            return Add((IEnumerable)items);
        }

        public EnvironmentScope Add(IEnumerable items)
        {
            foreach (var item in items)
            {
                var collection = item as IEnumerable;
                if (collection != null)
                {
                    Add(collection);
                }
                else
                {
                    if (item != null && _items.Contains(item) == false)
                        _items.Insert(0, item);
                }
            }
            return this;
        }

        public EnvironmentScope Remove(params object[] items)
        {
            return Remove((IEnumerable)items);
        }

        public EnvironmentScope Remove(IEnumerable items)
        {
            foreach (var item in items)
                _items.Remove(item);
            return this;
        }

        #endregion

        #region Key/Values

        public TItem GetKey<TItem>(string key) where TItem : class
        {
            return GetKey(key, typeof(TItem)) as TItem;
        }

        public object GetKey(string key, Type type)
        {
            object value;
            if (_keyValues.TryGetValue(key, out value) && type.IsInstanceOfType(value))
                return value;
            return _parentScope?.GetKey(key, type);
        }

        public bool HasKey(string key, Type type)
        {
            object value;
            if (_keyValues.TryGetValue(key, out value) && type.IsInstanceOfType(value))
                return true;
            return _parentScope != null && _parentScope.HasKey(key, type);
        }

        public EnvironmentScope SetKey(string key, object value)
        {
            _keyValues[key] = value;
            return this;
        }

        public object this[string key]
        {
            set { SetKey(key, value); }
        }

        #endregion

        internal static EnvironmentScope GetAmbientScope()
        {
            var instanceIdentifier = CallContext.LogicalGetData(AmbientScopeKey) as InstanceIdentifier;
            if (instanceIdentifier == null)
                return null;

            EnvironmentScope ambientScope;
            return AppDomainInstances.TryGetValue(instanceIdentifier, out ambientScope)
                 ? ambientScope 
                 : null;
        }

        internal static EnvironmentScope RequireAmbientScope()
        {
            return GetAmbientScope() ?? new EnvironmentScope();
        }

        internal static void SetAmbientScope(EnvironmentScope newAmbientScope)
        {
            var current = CallContext.LogicalGetData(AmbientScopeKey) as InstanceIdentifier;
            if (current == newAmbientScope._instanceIdentifier)
                return;

            CallContext.LogicalSetData(AmbientScopeKey, newAmbientScope._instanceIdentifier);

            AppDomainInstances.GetValue(newAmbientScope._instanceIdentifier, key => newAmbientScope);
        }

        internal static void RemoveAmbientScope()
        {
            var current = CallContext.LogicalGetData(AmbientScopeKey) as InstanceIdentifier;
            CallContext.LogicalSetData(AmbientScopeKey, null);

            if (current != null)
                AppDomainInstances.Remove(current);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            var currentAmbientScope = GetAmbientScope();
            if (currentAmbientScope != this)
                throw new InvalidOperationException("EnvironmentScope instances must be disposed of in the order in which they were created!");

            InternalDispose();

            if (_parentScope != null && _parentScope.Implied)
                _parentScope.Dispose();
         
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void InternalDispose()
        {
            RemoveAmbientScope();

            if (_parentScope != null)
                SetAmbientScope(_parentScope);

            _disposed = true;
        }

        ~EnvironmentScope()
        {
            if (GetAmbientScope() == this)
                InternalDispose();
            _disposed = true;
        }

        private Array GetArray(Type itemType, Type elementType)
        {
            var elements = _items.Where(elementType.IsInstanceOfType).ToList();
            var parentArray = _parentScope?.GetArray(itemType, elementType) as object[];
            if (parentArray != null && parentArray.Length > 0)
                elements.AddRange(parentArray);
            var array = Array.CreateInstance(elementType, elements.Count);
            for (var idx = 0; idx < elements.Count; ++idx)
                array.SetValue(elements[idx], idx);
            return array;
        }

        private IList GetList(Type itemType)
        {
            var elementType = itemType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)listType.CreateInstance<object>();
            foreach (var element in _items.Where(elementType.IsInstanceOfType))
                list.Add(element);
            if (_parentScope == null) return list;
            var parentList = _parentScope.GetList(itemType);
            if (parentList == null || parentList.Count <= 0) return list;
            foreach (var parentItem in parentList)
                list.Add(parentItem);
            return list;
        }

        private static readonly Type[] ListTypes = { typeof(IList<>), typeof(List<>) };

        internal class InstanceIdentifier : MarshalByRefObject {}
    }
}
