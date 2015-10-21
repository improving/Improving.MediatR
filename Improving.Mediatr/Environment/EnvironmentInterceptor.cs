namespace Improving.MediatR.Environment
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Castle.Core;
    using Castle.Core.Interceptor;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Context;
    using Castle.DynamicProxy;

    public class EnvironmentInterceptor : IInterceptor, IOnBehalfAware
    {
        private readonly IKernelInternal _kernel;
        private ComponentModel _componentModel;

        public EnvironmentInterceptor(IKernel kernel)
        {
            _kernel = kernel as IKernelInternal;
        }

        public void SetInterceptedComponentModel(ComponentModel target)
        {
            _componentModel = target;
        }

        public void Intercept(IInvocation invocation)
        {
            var envScope = EnvironmentScope.GetAmbientScope();
            if (envScope != null)  
                BindEnvironment(invocation.Proxy, envScope);
            invocation.Proceed(); 
        }

        private void BindEnvironment(object instance, EnvironmentScope envScope)
        {
            var resolver = _kernel.Resolver;
            var context  = CreationContext.CreateEmpty();
            instance     = ProxyUtil.GetUnproxiedInstance(instance);

            foreach (var property in _componentModel.Properties
                .Where(property => envScope.Contains(property.Property.PropertyType)))
            {
                try
                {
                    var value = resolver.Resolve(context, context.Handler, _componentModel, property.Dependency);
                    if (value == null) continue;  
                    try
                    {
                        var setMethod = property.Property.GetSetMethod();
                        setMethod.Invoke(instance, new[] {value});
                    }
                    catch
                    {
                        // ignore
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    #region EnvironmentProxyGenerationHook

    public class EnvironmentProxyGenerationHook : IProxyGenerationHook
    {
        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            var name = methodInfo.Name;
            return name.StartsWith("Apply")
                || name.StartsWith("Handle")
                || name.StartsWith("Validate");
        }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
        }

        public void MethodsInspected()
        {
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj is EnvironmentProxyGenerationHook;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    #endregion
}
