namespace Improving.MediatR
{
    using System;
    using System.Linq;
    using Castle.MicroKernel;

    internal class OpenGenericHandlersFilter : IHandlersFilter
    {
        private readonly Type _openRequestType;
        private readonly Type _openHandlerType;

        public OpenGenericHandlersFilter(Type openRequestType, Type openHandlerType)
        {
            _openRequestType = openRequestType;
            _openHandlerType = openHandlerType;
        }

        public bool HasOpinionAbout(Type service)
        {
            if (!service.IsGenericType)
                return false;

            var arguments = service.GetGenericArguments();
            return (arguments.Length == 2 && arguments[0].IsGenericType
                    && arguments[0].GetGenericTypeDefinition() == _openRequestType);
        }

        public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
        {
            var handler = handlers.Where(IsGenericDefinition).FirstOrDefault();
            return handler != null ? new[] { handler } : handlers;
        }

        private bool IsGenericDefinition(IHandler handler)
        {
            var type = handler.ComponentModel.Implementation;
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == _openHandlerType)
                    return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}
