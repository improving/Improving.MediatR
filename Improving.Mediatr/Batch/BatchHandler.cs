namespace Improving.MediatR.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Castle.Core;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Context;
    using Castle.MicroKernel.Handlers;
    using global::MediatR;

    public class BatchHandler<TPayload, TResponse>
        : IAsyncRequestHandler<BatchOf<TPayload, TResponse>, BatchResponse<TResponse>>,
          IRequireGenericMatching<BatchHandlerGenericCloser>
        where TPayload  : Request.WithResponse<TResponse>
        where TResponse : class
    {
        private readonly IMediator _mediator;

        public BatchHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<BatchResponse<TResponse>> Handle(BatchOf<TPayload, TResponse> batchOf)
        {
            return HandleBatch(batchOf, payload => _mediator.SendAsync(payload));
        }

        protected virtual async Task<BatchResponse<TResponse>> HandleBatch(
            BatchOf<TPayload, TResponse> batchOf,
            Func<TPayload, Task<TResponse>> sendPayload)
        {
            var responses = new List<TResponse>();
            if (batchOf.Batch != null)
            {
                foreach (var payload in batchOf.Batch)
                {
                    var response = await sendPayload(payload);
                    if (response != null)
                        responses.Add(response);
                }
            }
            return new BatchResponse<TResponse>(responses.ToArray());
        }
    }

    class BatchHandlerGenericCloser : IGenericImplementationMatchingStrategy
    {
        public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
        {
            var requestArgs = context.RequestedType.GetGenericArguments()[0];
            return requestArgs.GetGenericArguments();
        }
    }

    internal class BatchHandlerFilter : IHandlersFilter
    {
        public bool HasOpinionAbout(Type service)
        {
            if (!service.IsGenericType)
                return false;

            var arguments = service.GetGenericArguments();
            return (arguments.Length == 2 && arguments[0].IsGenericType
                    && arguments[0].GetGenericTypeDefinition() == typeof (BatchOf<,>));
        }

        public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
        {
            var handler = handlers.Where(IsBatchHandler).FirstOrDefault();
            return handler != null ? new[] { handler } : handlers;
        }

        private static bool IsBatchHandler(IHandler handler)
        {
            var type = handler.ComponentModel.Implementation;
            while (type != null && type != typeof (object))
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof (BatchHandler<,>))
                    return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}
