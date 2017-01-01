using System;
using Improving.MediatR.Environment;

namespace Improving.MediatR
{
    public static class Env
    {
        public static void Use<T>(T item)
        {
            var scope = EnvironmentScope.GetAmbientScope();
            if (scope == null)
                throw new InvalidOperationException("Use can only be called within an EnvironmentScope.");
            scope.Add(item);
        }
    }
}
