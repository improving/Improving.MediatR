namespace Improving.MediatR.Environment
{
    using Castle.Core;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Context;

    public class EnvironmentResolver : ISubDependencyResolver
    {
        public bool CanResolve(CreationContext context,
            ISubDependencyResolver contextHandlerResolver, ComponentModel model,
            DependencyModel dependency)
        {
            var envScope = EnvironmentScope.GetAmbientScope();
            if (envScope == null) return false;

            var key  = dependency.DependencyKey;
            var type = dependency.TargetItemType;
            return envScope.HasKey(key, type) || envScope.Contains(type);
        }

        public object Resolve(CreationContext context,
            ISubDependencyResolver contextHandlerResolver, ComponentModel model,
            DependencyModel dependency)
        {
            var envScope = EnvironmentScope.GetAmbientScope();
            if (envScope == null) return false;

            var key  = dependency.DependencyKey;
            var type = dependency.TargetItemType;
            return envScope.GetKey(key, type) ?? envScope.Get(type);
        }
    }
}
