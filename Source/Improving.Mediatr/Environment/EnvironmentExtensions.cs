namespace Improving.MediatR
{
    using Environment;
    using global::MediatR;

    public static class EnvironmentExtensions
    {
        public static T Use<T, E>(this T mediator, E item) where T : IMediator
        {
            var scope = EnvironmentScope.GetAmbientScope()
                     ?? new EnvironmentScope { Implied = true };
            if (item == null)
                scope.Clear(typeof(E));
            else
                scope.Add(item);
            return mediator;
        }

        public static T Key<T>(this T mediator, string key, object value) where T : IMediator
        {
            var scope = EnvironmentScope.GetAmbientScope()
                     ?? new EnvironmentScope { Implied = true };
            scope[key] = value;
            return mediator;
        }
    }
}
