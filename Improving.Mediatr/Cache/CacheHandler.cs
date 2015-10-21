namespace Improving.MediatR.Cache
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using Castle.Core;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Context;
    using Castle.MicroKernel.Handlers;
    using global::MediatR;

    public class CacheHandler<TResponse>
        : IAsyncRequestHandler<Cached<TResponse>, TResponse>,
          IRequireGenericMatching<CachedHandlerGenericCloser>
        where TResponse : class
    {
        private readonly IMediator _mediator;

        private static readonly 
            ConcurrentDictionary<Request.WithResponse<TResponse>, CacheResponse> Cache
            = new ConcurrentDictionary<Request.WithResponse<TResponse>, CacheResponse>();

        public CacheHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<TResponse> Handle(Cached<TResponse> request)
        {
            return Cache.AddOrUpdate(
                request.Request,   // actual request
                RefreshResponse,   // add first time
                (req, cached) =>   // update if stale or invalid
                    (cached.Response.Status == TaskStatus.Faulted  ||
                     cached.Response.Status == TaskStatus.Canceled ||
                     DateTime.UtcNow >= cached.LastUpdated + request.TimeToLive)
                ? RefreshResponse(req)
                : cached).Response;
        }

        private CacheResponse RefreshResponse(Request.WithResponse<TResponse> request)
        {
            return new CacheResponse
            {
                Response    = _mediator.SendAsync(request),
                LastUpdated = DateTime.UtcNow
            };
        }

        struct CacheResponse
        {
            public Task<TResponse> Response;
            public DateTime        LastUpdated;
        }
    }

    class CachedHandlerGenericCloser : IGenericImplementationMatchingStrategy
    {
        public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
        {
            var requestArgs = context.RequestedType.GetGenericArguments()[0];
            return requestArgs.GetGenericArguments();
        }
    }

    internal class CacheHandlerFilter : IHandlersFilter
    {
        public bool HasOpinionAbout(Type service)
        {
            if (!service.IsGenericType)
                return false;

            var arguments = service.GetGenericArguments();
            return (arguments.Length == 2 && arguments[0].IsGenericType
                    && arguments[0].GetGenericTypeDefinition() == typeof (Cached<>));
        }

        public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
        {
            var handler = handlers.Where(IsCachedHandler).FirstOrDefault();
            return handler != null ? new[] { handler } : handlers;
        }

        private static bool IsCachedHandler(IHandler handler)
        {
            var type = handler.ComponentModel.Implementation;
            while (type != null && type != typeof (object))
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof (CacheHandler<>))
                    return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}
