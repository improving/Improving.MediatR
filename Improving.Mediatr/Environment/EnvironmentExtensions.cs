namespace Improving.MediatR
{
    using System;
    using Environment;
    using global::MediatR;

    public static class EnvironmentExtensions
    {
        public static T With<T>(this T mediator, params object[] items) where T : IMediator
        {
            var scope = EnvironmentScope.GetAmbientScope()
                     ?? new EnvironmentScope { Implied = true };
            scope.Add(items);
            return mediator;
        }

        public static T WithKey<T>(this T mediator, string key, object value) where T : IMediator
        {
            var scope = EnvironmentScope.GetAmbientScope()
                     ?? new EnvironmentScope { Implied = true };
            scope[key] = value;
            return mediator;
        }

        public static T Use<T>(this T target, params object[] items) where T : class
        {
            var scope = EnvironmentScope.GetAmbientScope();
            if (scope == null)
                throw new InvalidOperationException("Use can only be called within an EnvironmentScope.");
            scope.Add(items);
            return target;
        }
    }
}
