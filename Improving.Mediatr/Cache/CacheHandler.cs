namespace Improving.MediatR.Cache
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using global::MediatR;

    public class CacheHandler<TResponse>
        : IAsyncRequestHandler<Cached<TResponse>, TResponse>,
          IRequireGenericMatching<HandlerGenericCloser>
    {
        private readonly IMediator _mediator;

        private static readonly 
            ConcurrentDictionary<IAsyncRequest<TResponse>, CacheResponse> Cache
            = new ConcurrentDictionary<IAsyncRequest<TResponse>, CacheResponse>();

        public CacheHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<TResponse> Handle(Cached<TResponse> request)
        {
            if (request.Request == null)
                return Task.FromResult(default(TResponse));

            if (request.Invalidate)
            {
                CacheResponse cached;
                return Cache.TryRemove(request.Request, out cached)
                        ? cached.Response
                        : Task.FromResult(default(TResponse));
            }

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

        private CacheResponse RefreshResponse(IAsyncRequest<TResponse> request)
        {
            return new CacheResponse
            {
                Response    = _mediator.SendAsync(request),
                LastUpdated = DateTime.UtcNow
            };
        }

        private struct CacheResponse
        {
            public Task<TResponse> Response;
            public DateTime        LastUpdated;
        }
    }
}
