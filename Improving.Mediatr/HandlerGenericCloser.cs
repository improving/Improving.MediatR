namespace Improving.MediatR
{
    using System;
    using Castle.Core;
    using Castle.MicroKernel.Context;
    using Castle.MicroKernel.Handlers;

    class HandlerGenericCloser : IGenericImplementationMatchingStrategy
    {
        public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
        {
            var requestArgs = context.RequestedType.GetGenericArguments()[0];
            return requestArgs.GetGenericArguments();
        }
    }
}
